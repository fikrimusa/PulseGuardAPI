using FluentAssertions;
using PulseGuard.Api.Models;
using PulseGuard.Api.Services;

namespace PulseGuard.Api.Tests;

public sealed class AlertStateServiceTests
{
    [Fact]
    public async Task UpdateAsync_creates_one_open_alert_after_three_consecutive_failures_and_resolves_it_on_recovery()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var monitor = new Monitor { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Test", Url = "https://example.test", IsActive = true };
        dbContext.Monitors.Add(monitor);
        await dbContext.SaveChangesAsync();
        var service = new AlertStateService();

        for (var failure = 1; failure <= 4; failure++)
        {
            var check = new MonitorCheck
            {
                Id = Guid.NewGuid(), MonitorId = monitor.Id, CheckedAt = DateTime.UtcNow.AddSeconds(failure),
                IsSuccess = false, ErrorMessage = "Expected HTTP 200 but received HTTP 500."
            };
            dbContext.MonitorChecks.Add(check);
            await dbContext.SaveChangesAsync();
            await service.UpdateAsync(dbContext, monitor, check, CancellationToken.None);
        }

        dbContext.Alerts.Should().ContainSingle();
        var alert = dbContext.Alerts.Single();
        alert.Status.Should().Be(AlertStatus.OPEN);
        alert.FailureCount.Should().Be(4);

        var recovery = new MonitorCheck { Id = Guid.NewGuid(), MonitorId = monitor.Id, CheckedAt = DateTime.UtcNow.AddMinutes(1), IsSuccess = true };
        dbContext.MonitorChecks.Add(recovery);
        await dbContext.SaveChangesAsync();
        await service.UpdateAsync(dbContext, monitor, recovery, CancellationToken.None);

        alert.Status.Should().Be(AlertStatus.RESOLVED);
        alert.ResolvedAt.Should().Be(recovery.CheckedAt);
    }
}
