using InfoTrack.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoTrack.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Solicitor> Solicitors => Set<Solicitor>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<SearchRecord> SearchRecords => Set<SearchRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>().HasData(
            new Location { Id = 1, Name = "London",     IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Location { Id = 2, Name = "Birmingham", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Location { Id = 3, Name = "Leeds",      IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Location { Id = 4, Name = "Manchester", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Location { Id = 5, Name = "Sheffield",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Location { Id = 6, Name = "Bradford",   IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Location { Id = 7, Name = "Liverpool",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Location { Id = 8, Name = "Bristol",    IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Solicitor>()
            .HasOne(s => s.SearchRecord)
            .WithMany(r => r.Solicitors)
            .HasForeignKey(s => s.SearchRecordId);
    }
}
