using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ServiceDesk.Api.Data;
using ServiceDesk.Api.Domain;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ServiceDesk.Tests;

public sealed class ApiIntegrationTests : IClassFixture<ServiceDeskFactory>
{
    private readonly ServiceDeskFactory factory;
    public ApiIntegrationTests(ServiceDeskFactory factory) => this.factory = factory;

    [Fact]
    public async Task RequestersSeeOnlyTheirOwnTickets()
    {
        using var client = await SignedInClient("requester1@example.test");
        var page = await client.GetFromJsonAsync<JsonElement>("/api/tickets?pageSize=100");
        var items = page.GetProperty("items").EnumerateArray().ToArray();
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Equal("Alex Morgan", item.GetProperty("requester").GetString()));

        using var other = await SignedInClient("requester2@example.test");
        var ownId = items[0].GetProperty("id").GetInt64();
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/tickets/{ownId}")).StatusCode);
    }

    [Fact]
    public async Task TechnicianCannotReadUnassignedTicketOrAssignWork()
    {
        using var client = await SignedInClient("technician1@example.test");
        var page = await client.GetFromJsonAsync<JsonElement>("/api/tickets?pageSize=100");
        Assert.All(page.GetProperty("items").EnumerateArray(), item => Assert.Equal("Jordan Lee", item.GetProperty("technician").GetString()));
        var csrf = await CsrfToken(client);
        var response = await client.PatchAsJsonAsync("/api/tickets/1/assignment", new { technicianId = Guid.NewGuid() }, new JsonSerializerOptions(), csrf);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateRequiresCsrfAndPersistsHistoryWithTicket()
    {
        using var client = await SignedInClient("requester1@example.test");
        var payload = new { title = "Integration test request", description = "A reproducible training test", categoryId = 1 };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/tickets", payload)).StatusCode);
        var csrf = await CsrfToken(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/tickets") { Content = JsonContent.Create(payload) };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        var created = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetInt64();
        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/tickets/{id}");
        Assert.Contains(detail.GetProperty("history").EnumerateArray(), entry => entry.GetProperty("action").GetString() == "Created");
    }

    private async Task<HttpClient> SignedInClient(string email)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var csrf = await CsrfToken(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "Training!2026" })
        };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return client;
    }

    private static async Task<string> CsrfToken(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        return response.GetProperty("token").GetString()!;
    }
}

public sealed class ServiceDeskFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__ServiceDesk")))
            throw new InvalidOperationException("Integration tests require ConnectionStrings__ServiceDesk pointing to a disposable PostgreSQL database.");
        using var scope = Services.CreateScope();
        await SeedData.ApplyAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), true);
    }

    public new async Task DisposeAsync() => await base.DisposeAsync();
}

internal static class HttpClientPatchExtensions
{
    public static async Task<HttpResponseMessage> PatchAsJsonAsync(this HttpClient client, string url, object payload,
        JsonSerializerOptions _, string csrf)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(payload) };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        return await client.SendAsync(request);
    }
}
