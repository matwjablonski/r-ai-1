using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;
using ServiceDesk.Api.Domain;
using ServiceDesk.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ServiceDesk")
    ?? throw new InvalidOperationException("ConnectionStrings:ServiceDesk is required");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "servicedesk.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "servicedesk.csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    await SeedData.ApplyAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), app.Environment.IsEnvironment("Training"));
    return;
}

app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(requestId) || requestId.Length > 80) requestId = context.TraceIdentifier;
    context.Response.Headers["X-Request-ID"] = requestId;
    try
    {
        await next(context);
    }
    catch (ApiException error)
    {
        context.Response.StatusCode = error.Status;
        await Results.Problem(title: error.Message, statusCode: error.Status).ExecuteAsync(context);
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "Request {RequestId} failed", requestId);
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            await Results.Problem(title: "Unexpected server error", statusCode: 500).ExecuteAsync(context);
        }
    }
    finally
    {
        stopwatch.Stop();
        app.Logger.LogInformation("{Method} {Path} {Status} {ElapsedMs}ms request={RequestId}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, requestId);
    }
});
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api") &&
        (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) ||
         HttpMethods.IsPatch(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method)))
    {
        try
        {
            await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = 400;
            await Results.Problem(title: "Invalid CSRF token", statusCode: 400).ExecuteAsync(context);
            return;
        }
    }
    await next(context);
});

app.MapOpenApi("/openapi/v1.json");
app.MapGet("/api/health", async (AppDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "healthy" }) : Results.StatusCode(503));

app.MapGet("/api/auth/csrf", (HttpContext context, IAntiforgery antiforgery) =>
    Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken }));
app.MapPost("/api/auth/login", async (LoginRequest request, AppDbContext db, IPasswordHasher<AppUser> hasher,
    HttpContext context, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        return Results.Problem(title: "Invalid email or password", statusCode: 401);
    var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant(), ct);
    if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        return Results.Problem(title: "Invalid email or password", statusCode: 401);
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.DisplayName),
        new Claim(ClaimTypes.Role, user.Role.ToString()) };
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    return Results.Ok(new UserView(user.Id, user.Email, user.DisplayName, user.Role));
});
app.MapPost("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.NoContent();
}).RequireAuthorization();
app.MapGet("/api/auth/me", (HttpContext context) => Results.Ok(new
{
    id = CallerFrom(context).Id,
    displayName = context.User.Identity?.Name,
    role = CallerFrom(context).Role.ToString()
})).RequireAuthorization();

var api = app.MapGroup("/api").RequireAuthorization();
api.MapGet("/categories", async (AppDbContext db, CancellationToken ct) =>
    await db.Categories.AsNoTracking().OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync(ct));
api.MapGet("/technicians", async (AppDbContext db, HttpContext context, CancellationToken ct) =>
{
    if (CallerFrom(context).Role != UserRole.Coordinator) return Results.Forbid();
    var users = await db.Users.AsNoTracking().Where(x => x.Role == UserRole.Technician)
        .OrderBy(x => x.DisplayName).Select(x => new { x.Id, x.DisplayName }).ToListAsync(ct);
    return Results.Ok(users);
});
api.MapGet("/tickets", async (TicketService service, HttpContext context, string? search, TicketStatus? status,
    TicketPriority? priority, string? sort, int? page, int? pageSize, CancellationToken ct) =>
    Results.Ok(await service.ListAsync(CallerFrom(context), search, status, priority, sort ?? "newest", page ?? 1, pageSize ?? 20, ct)));
api.MapGet("/tickets/{id:long}", async (long id, TicketService service, HttpContext context, CancellationToken ct) =>
    Results.Ok(await service.DetailAsync(id, CallerFrom(context), ct)));
api.MapPost("/tickets", async (CreateTicketRequest request, TicketService service, HttpContext context, CancellationToken ct) =>
{
    var id = await service.CreateAsync(request, CallerFrom(context), ct);
    return Results.Created($"/api/tickets/{id}", new { id });
});
api.MapPost("/tickets/{id:long}/comments", async (long id, AddCommentRequest request, TicketService service, HttpContext context, CancellationToken ct) =>
{
    await service.AddCommentAsync(id, request, CallerFrom(context), ct);
    return Results.NoContent();
});
api.MapPatch("/tickets/{id:long}/assignment", async (long id, AssignRequest request, TicketService service, HttpContext context, CancellationToken ct) =>
{
    await service.AssignAsync(id, request, CallerFrom(context), ct);
    return Results.NoContent();
});
api.MapPatch("/tickets/{id:long}/priority", async (long id, PriorityRequest request, TicketService service, HttpContext context, CancellationToken ct) =>
{
    await service.SetPriorityAsync(id, request, CallerFrom(context), ct);
    return Results.NoContent();
});
api.MapPatch("/tickets/{id:long}/status", async (long id, StatusRequest request, TicketService service, HttpContext context, CancellationToken ct) =>
{
    await service.SetStatusAsync(id, request, CallerFrom(context), ct);
    return Results.NoContent();
});
api.MapGet("/dashboard", async (TicketService service, HttpContext context, CancellationToken ct) =>
    Results.Ok(await service.DashboardAsync(CallerFrom(context), ct)));

app.Run();

static Caller CallerFrom(HttpContext context)
{
    var id = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    var role = context.User.FindFirstValue(ClaimTypes.Role);
    if (!Guid.TryParse(id, out var userId) || !Enum.TryParse<UserRole>(role, out var parsedRole))
        throw new ApiException(401, "Authentication required");
    return new Caller(userId, parsedRole);
}

public sealed record LoginRequest(string Email, string Password);
public sealed record UserView(Guid Id, string Email, string DisplayName, UserRole Role);
public partial class Program;
