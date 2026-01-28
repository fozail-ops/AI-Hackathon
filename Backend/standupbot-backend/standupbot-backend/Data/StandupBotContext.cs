using Microsoft.EntityFrameworkCore;
using standupbot_backend.Data.Entities;
using standupbot_backend.Data.Enums;

namespace standupbot_backend.Data;

public class StandupBotContext(DbContextOptions<StandupBotContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<Standup> Standups { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.HasOne(e => e.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Standup configuration
        modelBuilder.Entity<Standup>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();
            entity.Property(e => e.BlockerStatus)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Standups)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Seed sample data per PRD
        SeedData(modelBuilder);
    }
    
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Use a fixed date for seed data to avoid PendingModelChangesWarning
        // EF Core requires static values in HasData - DateTime.UtcNow would change on each build
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        // Seed Team
        modelBuilder.Entity<Team>().HasData(
            new Team { Id = 1, Name = "Product Engineering", CreatedAt = seedDate }
        );
        
        // Seed Users per PRD sample data
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Alice Johnson", Email = "alice.johnson@company.com", Role = UserRole.Lead, TeamId = 1, CreatedAt = seedDate },
            new User { Id = 2, Name = "Bob Smith", Email = "bob.smith@company.com", Role = UserRole.Member, TeamId = 1, CreatedAt = seedDate },
            new User { Id = 3, Name = "Carol White", Email = "carol.white@company.com", Role = UserRole.Member, TeamId = 1, CreatedAt = seedDate },
            new User { Id = 4, Name = "David Brown", Email = "david.brown@company.com", Role = UserRole.Member, TeamId = 1, CreatedAt = seedDate },
            new User { Id = 5, Name = "Eve Davis", Email = "eve.davis@company.com", Role = UserRole.Member, TeamId = 1, CreatedAt = seedDate }
        );
    }
}
