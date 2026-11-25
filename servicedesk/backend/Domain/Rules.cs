namespace ServiceDesk.Api.Domain;

public static class Rules
{
    public static int ResolutionHours(TicketPriority priority) => priority switch
    {
        TicketPriority.Low => 48,
        TicketPriority.Medium => 24,
        TicketPriority.High => 8,
        TicketPriority.Critical => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(priority))
    };

    public static DateTimeOffset DueAt(DateTimeOffset createdAt, TicketPriority priority) =>
        createdAt.AddHours(ResolutionHours(priority));

    public static bool IsAtRisk(Ticket ticket, DateTimeOffset now) =>
        ticket.Status is not (TicketStatus.Resolved or TicketStatus.Closed)
        && ticket.DueAt > now
        && ticket.DueAt - now <= TimeSpan.FromHours(ResolutionHours(ticket.Priority) * 0.25);

    public static bool IsOverdue(Ticket ticket, DateTimeOffset now) =>
        ticket.Status is not (TicketStatus.Resolved or TicketStatus.Closed) && ticket.DueAt < now;

    public static bool CanTransition(TicketStatus current, TicketStatus next, UserRole role, bool assignedToActor, bool hasTechnician) =>
        (current, next) switch
        {
            (TicketStatus.Open, TicketStatus.InProgress) => hasTechnician && (role == UserRole.Coordinator || assignedToActor),
            (TicketStatus.InProgress, TicketStatus.Resolved) => role == UserRole.Coordinator || assignedToActor,
            (TicketStatus.Resolved, TicketStatus.Closed) => role == UserRole.Coordinator,
            (TicketStatus.Resolved, TicketStatus.InProgress) => hasTechnician && role == UserRole.Coordinator,
            _ => false
        };
}
