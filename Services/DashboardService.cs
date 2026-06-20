using Microsoft.EntityFrameworkCore;
using PulseGuard.Api.Data;
using PulseGuard.Api.DTOs;
using PulseGuard.Api.Models;

namespace PulseGuard.Api.Services;

public sealed class DashboardService(AppDbContext dbContext)
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var monitors = await dbContext.Monitors
            .AsNoTracking()
            .Where(monitor => monitor.UserId == userId)
            .Select(monitor => new { monitor.Id, monitor.Name, monitor.IsActive })
            .ToListAsync(cancellationToken);

        var monitorIds = monitors.Select(monitor => monitor.Id).ToArray();
        var latestChecks = await dbContext.MonitorChecks
            .AsNoTracking()
            .Where(check => monitorIds.Contains(check.MonitorId))
            .OrderByDescending(check => check.CheckedAt)
            .ToListAsync(cancellationToken);
        var latestCheckByMonitor = latestChecks
            .GroupBy(check => check.MonitorId)
            .ToDictionary(group => group.Key, group => group.First());

        var userAlerts = await dbContext.Alerts
            .AsNoTracking()
            .Where(alert => alert.Monitor.UserId == userId)
            .OrderByDescending(alert => alert.CreatedAt)
            .ToListAsync(cancellationToken);

        var monitorNames = monitors.ToDictionary(monitor => monitor.Id, monitor => monitor.Name);
        var recentAlerts = userAlerts
            .Take(10)
            .Select(alert => new DashboardRecentAlertResponse(
                alert.Id,
                alert.MonitorId,
                monitorNames[alert.MonitorId],
                alert.CreatedAt,
                alert.Status,
                alert.Message,
                alert.FailureCount))
            .ToArray();
        var recentChecks = latestChecks
            .Take(10)
            .Select(check => new DashboardRecentMonitorCheckResponse(
                check.Id,
                check.MonitorId,
                monitorNames[check.MonitorId],
                check.CheckedAt,
                check.IsSuccess,
                check.StatusCode,
                check.ResponseTimeMs,
                check.ErrorMessage))
            .ToArray();
        var latestCheckValues = latestCheckByMonitor.Values.ToArray();

        return new DashboardSummaryResponse(
            monitors.Count,
            monitors.Count(monitor => monitor.IsActive),
            monitors.Count(monitor => !monitor.IsActive),
            latestCheckValues.Count(check => check.IsSuccess),
            latestCheckValues.Count(check => !check.IsSuccess),
            monitors.Count - latestCheckValues.Length,
            userAlerts.Count(alert => alert.Status == AlertStatus.OPEN),
            userAlerts.Count(alert => alert.Status == AlertStatus.ACKNOWLEDGED),
            userAlerts.Count(alert => alert.Status == AlertStatus.RESOLVED),
            latestCheckValues.Length == 0 ? null : latestCheckValues.Average(check => check.ResponseTimeMs),
            recentAlerts,
            recentChecks);
    }
}
