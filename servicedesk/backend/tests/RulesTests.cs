using ServiceDesk.Api.Domain;

namespace ServiceDesk.Tests;

public sealed class RulesTests
{
    [Theory]
    [InlineData(TicketPriority.Low, 48)]
    [InlineData(TicketPriority.Medium, 24)]
    [InlineData(TicketPriority.High, 8)]
    [InlineData(TicketPriority.Critical, 2)]
    public void PriorityDeterminesCalendarHours(TicketPriority priority, int hours)
    {
        var created = new DateTimeOffset(2026, 11, 24, 10, 0, 0, TimeSpan.Zero);
        Assert.Equal(created.AddHours(hours), Rules.DueAt(created, priority));
    }

    [Fact]
    public void ReopeningKeepsOriginalDeadlineAndNeedsCoordinator()
    {
        Assert.False(Rules.CanTransition(TicketStatus.Resolved, TicketStatus.InProgress, UserRole.Technician, true, true));
        Assert.True(Rules.CanTransition(TicketStatus.Resolved, TicketStatus.InProgress, UserRole.Coordinator, false, true));
    }

    [Fact]
    public void StartingWorkNeedsAssignedTechnician()
    {
        Assert.False(Rules.CanTransition(TicketStatus.Open, TicketStatus.InProgress, UserRole.Coordinator, false, false));
        Assert.True(Rules.CanTransition(TicketStatus.Open, TicketStatus.InProgress, UserRole.Technician, true, true));
    }

    [Fact]
    public void RiskIsCalculatedFromOriginalSlaWindow()
    {
        var created = new DateTimeOffset(2026, 11, 24, 10, 0, 0, TimeSpan.Zero);
        var ticket = new Ticket { Priority = TicketPriority.Medium, DueAt = Rules.DueAt(created, TicketPriority.Medium) };
        Assert.False(Rules.IsAtRisk(ticket, created.AddHours(17)));
        Assert.True(Rules.IsAtRisk(ticket, created.AddHours(18)));
        Assert.True(Rules.IsOverdue(ticket, created.AddHours(25)));
    }
}
