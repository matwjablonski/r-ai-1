using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Domain;

namespace ServiceDesk.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<TicketComment> Comments => Set<TicketComment>();
    public DbSet<TicketHistory> History => Set<TicketHistory>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        model.Entity<AppUser>().Property(x => x.Role).HasConversion<string>();
        model.Entity<Ticket>().Property(x => x.Priority).HasConversion<string>();
        model.Entity<Ticket>().Property(x => x.Status).HasConversion<string>();
        model.Entity<SlaPolicy>().Property(x => x.Priority).HasConversion<string>();
        model.Entity<SlaPolicy>().HasIndex(x => x.Priority).IsUnique();
        model.Entity<Ticket>().HasIndex(x => new { x.Status, x.CreatedAt });
        model.Entity<Ticket>().HasIndex(x => x.RequesterId);
        model.Entity<Ticket>().HasIndex(x => x.TechnicianId);
        model.Entity<Ticket>().HasOne(x => x.Requester).WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Ticket>().HasOne(x => x.Technician).WithMany().HasForeignKey(x => x.TechnicianId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<TicketComment>().HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<TicketHistory>().HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<TicketComment>().HasIndex(x => new { x.TicketId, x.CreatedAt });
        model.Entity<TicketHistory>().HasIndex(x => new { x.TicketId, x.CreatedAt });
    }
}
