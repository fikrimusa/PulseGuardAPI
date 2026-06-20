using Microsoft.EntityFrameworkCore;
using MonitorModel = PulseGuard.Api.Models.Monitor;

namespace PulseGuard.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MonitorModel> Monitors => Set<MonitorModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var monitor = modelBuilder.Entity<MonitorModel>();

        monitor.ToTable("monitors");
        monitor.HasKey(entity => entity.Id);

        monitor.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        monitor.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        monitor.Property(entity => entity.Url)
            .HasColumnName("url")
            .HasMaxLength(2048)
            .IsRequired();
        monitor.Property(entity => entity.CheckIntervalSeconds)
            .HasColumnName("check_interval_seconds")
            .IsRequired();
        monitor.Property(entity => entity.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        monitor.Property(entity => entity.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        monitor.Property(entity => entity.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
