using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Api.Domain;

public enum UserRole { Requester, Technician, Coordinator }
public enum TicketStatus { Open, InProgress, Resolved, Closed }
public enum TicketPriority { Low, Medium, High, Critical }

public sealed class AppUser
{
    public Guid Id { get; set; }
    [MaxLength(160)] public string Email { get; set; } = "";
    [MaxLength(120)] public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; }
}

public sealed class Category
{
    public int Id { get; set; }
    [MaxLength(80)] public string Name { get; set; } = "";
}

public sealed class SlaPolicy
{
    public int Id { get; set; }
    public TicketPriority Priority { get; set; }
    public int ResolutionHours { get; set; }
}

public sealed class Ticket
{
    public long Id { get; set; }
    [MaxLength(180)] public string Title { get; set; } = "";
    [MaxLength(6000)] public string Description { get; set; } = "";
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public Guid RequesterId { get; set; }
    public AppUser Requester { get; set; } = null!;
    public Guid? TechnicianId { get; set; }
    public AppUser? Technician { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public List<TicketComment> Comments { get; set; } = [];
    public List<TicketHistory> History { get; set; } = [];
}

public sealed class TicketComment
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public AppUser Author { get; set; } = null!;
    [MaxLength(4000)] public string Body { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class TicketHistory
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public Guid ActorId { get; set; }
    public AppUser Actor { get; set; } = null!;
    [MaxLength(80)] public string Action { get; set; } = "";
    [MaxLength(500)] public string Detail { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}
