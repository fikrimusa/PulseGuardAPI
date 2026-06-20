using Microsoft.EntityFrameworkCore;
using MonitorModel = PulseGuard.Api.Models.Monitor;
using UserModel = PulseGuard.Api.Models.User;

namespace PulseGuard.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MonitorModel> Monitors => Set<MonitorModel>();

    public DbSet<UserModel> Users => Set<UserModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<UserModel>();

        user.ToTable("users");
        user.HasKey(entity => entity.Id);
        user.HasIndex(entity => entity.Email).IsUnique();
        user.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        user.Property(entity => entity.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();
        user.Property(entity => entity.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();
        user.Property(entity => entity.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        var monitor = modelBuilder.Entity<MonitorModel>();

        monitor.ToTable("monitors");
        monitor.HasKey(entity => entity.Id);
        monitor.HasOne(entity => entity.User)
            .WithMany(entity => entity.Monitors)
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        monitor.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        monitor.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .IsRequired();
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
