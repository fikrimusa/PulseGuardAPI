using FluentAssertions;
using PulseGuard.Api.Models;
using PulseGuard.Api.Repositories;

namespace PulseGuard.Api.Tests;

public sealed class MonitorOwnershipTests
{
    [Fact]
    public async Task GetById_returns_null_for_a_monitor_owned_by_another_user()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var monitor = new Monitor { Id = Guid.NewGuid(), UserId = ownerId, Name = "Private", Url = "https://example.test" };
        dbContext.Monitors.Add(monitor);
        await dbContext.SaveChangesAsync();
        var repository = new MonitorRepository(dbContext);

        repository.GetById(monitor.Id, otherUserId).Should().BeNull();
        repository.GetById(monitor.Id, ownerId).Should().NotBeNull();
    }
}
