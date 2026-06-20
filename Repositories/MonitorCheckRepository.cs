using Microsoft.EntityFrameworkCore;
using PulseGuard.Api.Data;
using MonitorCheckModel = PulseGuard.Api.Models.MonitorCheck;

namespace PulseGuard.Api.Repositories;

public sealed class MonitorCheckRepository(AppDbContext dbContext)
{
    public IReadOnlyCollection<MonitorCheckModel> GetForMonitor(Guid monitorId)
    {
        return dbContext.MonitorChecks
            .AsNoTracking()
            .Where(check => check.MonitorId == monitorId)
            .OrderByDescending(check => check.CheckedAt)
            .ToArray();
    }
}
