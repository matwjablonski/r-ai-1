using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Domain;

namespace ServiceDesk.Api.Data;

public static class SeedData
{
    public static async Task ApplyAsync(AppDbContext db, bool seedDemo, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
        if (!seedDemo) return;
        if (await db.Users.AnyAsync(cancellationToken)) return;

        var users = new[]
        {
            User("requester1@example.test", "Alex Morgan", UserRole.Requester),
            User("requester2@example.test", "Sam Taylor", UserRole.Requester),
            User("technician1@example.test", "Jordan Lee", UserRole.Technician),
            User("technician2@example.test", "Casey Rivera", UserRole.Technician),
            User("technician3@example.test", "Jamie Kim", UserRole.Technician),
            User("coordinator@example.test", "Morgan Blake", UserRole.Coordinator)
        };
        var hasher = new PasswordHasher<AppUser>();
        foreach (var user in users) user.PasswordHash = hasher.HashPassword(user, "Training!2026");
        db.Users.AddRange(users);

        var categories = new[]
        {
            new Category { Name = "Accounts and access" },
            new Category { Name = "Hardware" },
            new Category { Name = "Software" },
            new Category { Name = "Network" }
        };
        db.Categories.AddRange(categories);
        db.SlaPolicies.AddRange(Enum.GetValues<TicketPriority>().Select(priority => new SlaPolicy
        {
            Priority = priority,
            ResolutionHours = Rules.ResolutionHours(priority)
        }));
        await db.SaveChangesAsync(cancellationToken);

        var titles = new[]
        {
            "Cannot access shared workspace", "Laptop battery drains quickly", "Application closes unexpectedly",
            "VPN connection fails", "Password reset needed", "Printer is unavailable",
            "Slow file synchronization", "Missing project permissions", "Audio device not detected",
            "Browser session expires early", "Software installation request", "Intermittent Wi-Fi connection"
        };
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 150; i++)
        {
            var created = now.AddHours(-(i % 54));
            var priority = (TicketPriority)(i % 4);
            var status = (TicketStatus)(i % 4);
            var requester = users[i % 2];
            var technician = status == TicketStatus.Open && i % 5 == 0 ? null : users[2 + i % 3];
            if (status != TicketStatus.Open && technician is null) technician = users[2];
            var ticket = new Ticket
            {
                Title = titles[i % titles.Length],
                Description = $"Synthetic training request #{i + 1}. Please investigate the reported behavior and document the outcome.",
                CategoryId = categories[i % categories.Length].Id,
                RequesterId = requester.Id,
                TechnicianId = technician?.Id,
                Priority = priority,
                Status = status,
                CreatedAt = created,
                UpdatedAt = created.AddMinutes(20),
                ResolvedAt = status is TicketStatus.Resolved or TicketStatus.Closed ? created.AddMinutes(35) : null,
                DueAt = Rules.DueAt(created, priority)
            };
            ticket.History.Add(new TicketHistory { ActorId = requester.Id, Action = "Created", Detail = "Ticket created", CreatedAt = created });
            if (technician is not null)
                ticket.History.Add(new TicketHistory { ActorId = users[5].Id, Action = "Assigned", Detail = $"Assigned to {technician.DisplayName}", CreatedAt = created.AddMinutes(10) });
            if (status != TicketStatus.Open)
                ticket.History.Add(new TicketHistory { ActorId = technician!.Id, Action = "StatusChanged", Detail = $"Status changed to {status}", CreatedAt = created.AddMinutes(20) });
            ticket.Comments.Add(new TicketComment { AuthorId = requester.Id, Body = "This issue can be reproduced on the training environment.", CreatedAt = created.AddMinutes(5) });
            db.Tickets.Add(ticket);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private static AppUser User(string email, string name, UserRole role) => new()
    {
        Id = Guid.NewGuid(), Email = email, DisplayName = name, Role = role
    };
}
