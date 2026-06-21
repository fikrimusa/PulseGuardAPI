using FluentAssertions;
using PulseGuard.Api.Data;
using PulseGuard.Api.Models;
using PulseGuard.Api.Services;

namespace PulseGuard.Api.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_uses_latest_check_and_excludes_other_users_data()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var up = new Monitor { Id = Guid.NewGuid(), UserId = userId, Name = "Up", Url = "https://up.test", IsActive = true };
        var unknown = new Monitor { Id = Guid.NewGuid(), UserId = userId, Name = "Unknown", Url = "https://unknown.test", IsActive = false };
        var other = new Monitor { Id = Guid.NewGuid(), UserId = otherUserId, Name = "Other", Url = "https://other.test", IsActive = true };
        dbContext.Monitors.AddRange(up, unknown, other);
        dbContext.MonitorChecks.AddRange(
            new MonitorCheck { Id = Guid.NewGuid(), MonitorId = up.Id, CheckedAt = DateTime.UtcNow.AddMinutes(-1), IsSuccess = false, ResponseTimeMs = 50 },
            new MonitorCheck { Id = Guid.NewGuid(), MonitorId = up.Id, CheckedAt = DateTime.UtcNow, IsSuccess = true, ResponseTimeMs = 20 },
            new MonitorCheck { Id = Guid.NewGuid(), MonitorId = other.Id, CheckedAt = DateTime.UtcNow, IsSuccess = false, ResponseTimeMs = 100 });
        dbContext.Alerts.AddRange(
            new Alert { Id = Guid.NewGuid(), MonitorId = up.Id, CreatedAt = DateTime.UtcNow, Status = AlertStatus.OPEN, Message = "Failure", FailureCount = 3, Monitor = up },
            new Alert { Id = Guid.NewGuid(), MonitorId = other.Id, CreatedAt = DateTime.UtcNow, Status = AlertStatus.OPEN, Message = "Other", FailureCount = 3, Monitor = other });
        await dbContext.SaveChangesAsync();

        var summary = await new DashboardService(dbContext).GetSummaryAsync(userId, CancellationToken.None);

        summary.TotalMonitors.Should().Be(2);
        summary.EnabledMonitors.Should().Be(1);
        summary.DisabledMonitors.Should().Be(1);
        summary.UpMonitors.Should().Be(1);
        summary.DownMonitors.Should().Be(0);
        summary.UnknownMonitors.Should().Be(1);
        summary.OpenAlerts.Should().Be(1);
        summary.AverageResponseTimeMs.Should().Be(20);
    }
}
