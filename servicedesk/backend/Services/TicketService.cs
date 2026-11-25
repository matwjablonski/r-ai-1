using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;
using ServiceDesk.Api.Domain;

namespace ServiceDesk.Api.Services;

public sealed record Caller(Guid Id, UserRole Role);
public sealed record TicketItem(long Id, string Title, string Category, TicketPriority Priority, TicketStatus Status,
    string Requester, string? Technician, DateTimeOffset CreatedAt, DateTimeOffset DueAt, bool AtRisk, bool Overdue);
public sealed record CommentItem(long Id, string Author, string Body, DateTimeOffset CreatedAt);
public sealed record HistoryItem(long Id, string Actor, string Action, string Detail, DateTimeOffset CreatedAt);
public sealed record TicketDetail(TicketItem Ticket, string Description, DateTimeOffset UpdatedAt,
    DateTimeOffset? ResolvedAt, IReadOnlyList<CommentItem> Comments, IReadOnlyList<HistoryItem> History);
public sealed record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);
public sealed record Dashboard(int Open, int InProgress, int AtRisk, int Overdue, int ResolvedToday);
public sealed record CreateTicketRequest(string Title, string Description, int CategoryId);
public sealed record AddCommentRequest(string Body);
public sealed record AssignRequest(Guid TechnicianId);
public sealed record PriorityRequest(TicketPriority Priority);
public sealed record StatusRequest(TicketStatus Status);

public sealed class ApiException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}

public sealed class TicketService(AppDbContext db, TimeProvider clock)
{
    private IQueryable<Ticket> VisibleTo(Caller caller)
    {
        var query = db.Tickets.AsQueryable();
        return caller.Role switch
        {
            UserRole.Requester => query.Where(x => x.RequesterId == caller.Id),
            UserRole.Technician => query.Where(x => x.TechnicianId == caller.Id),
            _ => query
        };
    }

    private async Task<Ticket> FindVisibleAsync(long id, Caller caller, CancellationToken ct)
    {
        var ticket = await VisibleTo(caller)
            .Include(x => x.Category).Include(x => x.Requester).Include(x => x.Technician)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return ticket ?? throw new ApiException(404, "Ticket not found");
    }

    private TicketItem Item(Ticket x) => new(x.Id, x.Title, x.Category.Name, x.Priority, x.Status,
        x.Requester.DisplayName, x.Technician?.DisplayName, x.CreatedAt, x.DueAt,
        Rules.IsAtRisk(x, clock.GetUtcNow()), Rules.IsOverdue(x, clock.GetUtcNow()));

    public async Task<Page<TicketItem>> ListAsync(Caller caller, string? search, TicketStatus? status,
        TicketPriority? priority, string sort, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Clamp(page, 1, 100_000);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = VisibleTo(caller).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Title, "%" + term + "%") || EF.Functions.ILike(x.Description, "%" + term + "%"));
        }
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (priority.HasValue) query = query.Where(x => x.Priority == priority.Value);
        var total = await query.CountAsync(ct);
        query = sort switch
        {
            "oldest" => query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            "due" => query.OrderBy(x => x.DueAt).ThenBy(x => x.Id),
            "priority" => query.OrderByDescending(x => x.Priority == TicketPriority.Critical ? 4 : x.Priority == TicketPriority.High ? 3 : x.Priority == TicketPriority.Medium ? 2 : 1).ThenByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
        };
        var tickets = await query.Include(x => x.Category).Include(x => x.Requester).Include(x => x.Technician)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new Page<TicketItem>(tickets.Select(Item).ToArray(), total, page, pageSize);
    }

    public async Task<TicketDetail> DetailAsync(long id, Caller caller, CancellationToken ct)
    {
        var ticket = await FindVisibleAsync(id, caller, ct);
        await db.Entry(ticket).Collection(x => x.Comments).Query().Include(x => x.Author)
            .OrderBy(x => x.CreatedAt).LoadAsync(ct);
        await db.Entry(ticket).Collection(x => x.History).Query().Include(x => x.Actor)
            .OrderBy(x => x.CreatedAt).LoadAsync(ct);
        return new TicketDetail(Item(ticket), ticket.Description, ticket.UpdatedAt, ticket.ResolvedAt,
            ticket.Comments.OrderBy(x => x.CreatedAt).Select(x => new CommentItem(x.Id, x.Author.DisplayName, x.Body, x.CreatedAt)).ToArray(),
            ticket.History.OrderBy(x => x.CreatedAt).Select(x => new HistoryItem(x.Id, x.Actor.DisplayName, x.Action, x.Detail, x.CreatedAt)).ToArray());
    }

    public async Task<long> CreateAsync(CreateTicketRequest request, Caller caller, CancellationToken ct)
    {
        if (caller.Role != UserRole.Requester) throw new ApiException(403, "Only requesters can create tickets");
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 180 ||
            string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length > 6000)
            throw new ApiException(400, "Title and description are required and must fit their limits");
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId, ct)) throw new ApiException(400, "Unknown category");
        var now = clock.GetUtcNow();
        var ticket = new Ticket
        {
            Title = request.Title.Trim(), Description = request.Description.Trim(), CategoryId = request.CategoryId,
            RequesterId = caller.Id, Priority = TicketPriority.Medium, Status = TicketStatus.Open,
            CreatedAt = now, UpdatedAt = now, DueAt = Rules.DueAt(now, TicketPriority.Medium)
        };
        ticket.History.Add(new TicketHistory { ActorId = caller.Id, Action = "Created", Detail = "Ticket created", CreatedAt = now });
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync(ct);
        return ticket.Id;
    }

    public async Task AssignAsync(long id, AssignRequest request, Caller caller, CancellationToken ct)
    {
        if (caller.Role != UserRole.Coordinator) throw new ApiException(403, "Coordinator role required");
        var ticket = await FindVisibleAsync(id, caller, ct);
        if (ticket.Status == TicketStatus.Closed) throw new ApiException(409, "Closed ticket cannot be assigned");
        var technician = await db.Users.FirstOrDefaultAsync(x => x.Id == request.TechnicianId && x.Role == UserRole.Technician, ct)
            ?? throw new ApiException(400, "Assignee must be a technician");
        ticket.TechnicianId = technician.Id;
        ticket.UpdatedAt = clock.GetUtcNow();
        AddHistory(ticket, caller, "Assigned", $"Assigned to {technician.DisplayName}");
        await db.SaveChangesAsync(ct);
    }

    public async Task SetPriorityAsync(long id, PriorityRequest request, Caller caller, CancellationToken ct)
    {
        if (caller.Role != UserRole.Coordinator) throw new ApiException(403, "Coordinator role required");
        if (!Enum.IsDefined(request.Priority)) throw new ApiException(400, "Unknown priority");
        var ticket = await FindVisibleAsync(id, caller, ct);
        if (ticket.Status == TicketStatus.Closed) throw new ApiException(409, "Closed ticket cannot change priority");
        ticket.Priority = request.Priority;
        ticket.DueAt = Rules.DueAt(ticket.CreatedAt, request.Priority);
        ticket.UpdatedAt = clock.GetUtcNow();
        AddHistory(ticket, caller, "PriorityChanged", $"Priority changed to {request.Priority}");
        await db.SaveChangesAsync(ct);
    }

    public async Task SetStatusAsync(long id, StatusRequest request, Caller caller, CancellationToken ct)
    {
        if (caller.Role == UserRole.Requester) throw new ApiException(403, "Requesters cannot change status");
        var ticket = await FindVisibleAsync(id, caller, ct);
        if (!Enum.IsDefined(request.Status) || !Rules.CanTransition(ticket.Status, request.Status, caller.Role,
            ticket.TechnicianId == caller.Id, ticket.TechnicianId.HasValue))
            throw new ApiException(409, "Status transition is not allowed");
        ticket.Status = request.Status;
        ticket.UpdatedAt = clock.GetUtcNow();
        ticket.ResolvedAt = request.Status is TicketStatus.Resolved or TicketStatus.Closed ? ticket.ResolvedAt ?? ticket.UpdatedAt : null;
        AddHistory(ticket, caller, "StatusChanged", $"Status changed to {request.Status}");
        await db.SaveChangesAsync(ct);
    }

    public async Task AddCommentAsync(long id, AddCommentRequest request, Caller caller, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 4000)
            throw new ApiException(400, "Comment must contain 1–4000 characters");
        var ticket = await FindVisibleAsync(id, caller, ct);
        var now = clock.GetUtcNow();
        ticket.Comments.Add(new TicketComment { AuthorId = caller.Id, Body = request.Body.Trim(), CreatedAt = now });
        ticket.History.Add(new TicketHistory { ActorId = caller.Id, Action = "Commented", Detail = "Comment added", CreatedAt = now });
        ticket.UpdatedAt = now;
        await db.SaveChangesAsync(ct);
    }

    public async Task<Dashboard> DashboardAsync(Caller caller, CancellationToken ct)
    {
        var tickets = await VisibleTo(caller).AsNoTracking().ToListAsync(ct);
        var now = clock.GetUtcNow();
        var today = now.UtcDateTime.Date;
        return new Dashboard(
            tickets.Count(x => x.Status == TicketStatus.Open),
            tickets.Count(x => x.Status == TicketStatus.InProgress),
            tickets.Count(x => Rules.IsAtRisk(x, now)),
            tickets.Count(x => Rules.IsOverdue(x, now)),
            tickets.Count(x => x.ResolvedAt?.UtcDateTime.Date == today));
    }

    private void AddHistory(Ticket ticket, Caller caller, string action, string detail) =>
        ticket.History.Add(new TicketHistory { ActorId = caller.Id, Action = action, Detail = detail, CreatedAt = clock.GetUtcNow() });
}
