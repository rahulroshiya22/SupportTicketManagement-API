using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.API.Models;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketStatusLog> TicketStatusLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Role>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.Name).HasConversion<string>().IsRequired();
                e.HasIndex(r => r.Name).IsUnique();
            });

            builder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Name).IsRequired().HasMaxLength(255);
                e.Property(u => u.Email).IsRequired().HasMaxLength(255);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Password).IsRequired().HasMaxLength(255);
                e.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Ticket>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.Title).IsRequired().HasMaxLength(255);
                e.Property(t => t.Description).IsRequired();
                e.Property(t => t.Status).HasConversion<string>().HasDefaultValue(TicketStatus.OPEN);
                e.Property(t => t.Priority).HasConversion<string>().HasDefaultValue(TicketPriority.MEDIUM);
                e.HasOne(t => t.CreatedBy).WithMany(u => u.CreatedTickets).HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.AssignedTo).WithMany(u => u.AssignedTickets).HasForeignKey(t => t.AssignedToId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TicketComment>(e =>
            {
                e.HasKey(c => c.Id);
                e.HasOne(c => c.Ticket).WithMany(t => t.Comments).HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.User).WithMany(u => u.Comments).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TicketStatusLog>(e =>
            {
                e.HasKey(l => l.Id);
                e.Property(l => l.OldStatus).HasConversion<string>().IsRequired();
                e.Property(l => l.NewStatus).HasConversion<string>().IsRequired();
                e.HasOne(l => l.Ticket).WithMany(t => t.StatusLogs).HasForeignKey(l => l.TicketId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(l => l.ChangedBy).WithMany(u => u.StatusChanges).HasForeignKey(l => l.ChangedById).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
