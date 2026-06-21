using FluentAssertions;
using PulseGuard.Api.Models;
using PulseGuard.Api.Repositories;
using PulseGuard.Api.Services;

namespace PulseGuard.Api.Tests;

public sealed class AlertAcknowledgementTests
{
    [Fact]
    public async Task Acknowledge_marks_an_owned_open_alert_as_acknowledged()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var monitor = new Monitor { Id = Guid.NewGuid(), UserId = userId, Name = "Owned", Url = "https://example.test" };
        var alert = new Alert
        {
            Id = Guid.NewGuid(), MonitorId = monitor.Id, Monitor = monitor, CreatedAt = DateTime.UtcNow,
            Status = AlertStatus.OPEN, Message = "Failure", FailureCount = 3
        };
        dbContext.AddRange(monitor, alert);
        await dbContext.SaveChangesAsync();

        var service = new AlertService(new AlertRepository(dbContext));
        var result = service.Acknowledge(alert.Id, userId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(AlertStatus.ACKNOWLEDGED);
        result.AcknowledgedAt.Should().NotBeNull();
    }
}
