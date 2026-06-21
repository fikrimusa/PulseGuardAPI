using Microsoft.EntityFrameworkCore;
using PulseGuard.Api.Data;
using AlertModel = PulseGuard.Api.Models.Alert;
using AlertStatusModel = PulseGuard.Api.Models.AlertStatus;
using MonitorCheckModel = PulseGuard.Api.Models.MonitorCheck;
using MonitorModel = PulseGuard.Api.Models.Monitor;

namespace PulseGuard.Api.Services;

public sealed class AlertStateService
{
    public async Task UpdateAsync(
        AppDbContext dbContext,
        MonitorModel monitor,
        MonitorCheckModel check,
        CancellationToken cancellationToken)
    {
        var activeAlert = await dbContext.Alerts
            .SingleOrDefaultAsync(alert =>
                alert.MonitorId == monitor.Id
                && (alert.Status == AlertStatusModel.OPEN || alert.Status == AlertStatusModel.ACKNOWLEDGED),
                cancellationToken);

        if (check.IsSuccess)
        {
            if (activeAlert is not null)
            {
                activeAlert.Status = AlertStatusModel.RESOLVED;
                activeAlert.ResolvedAt = check.CheckedAt;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var recentChecks = await dbContext.MonitorChecks
            .AsNoTracking()
            .Where(result => result.MonitorId == monitor.Id)
            .OrderByDescending(result => result.CheckedAt)
            .Select(result => result.IsSuccess)
            .ToListAsync(cancellationToken);
        var failureCount = recentChecks.TakeWhile(isSuccess => !isSuccess).Count();

        if (failureCount < 3)
        {
            return;
        }

        var message = $"Monitor failed {failureCount} consecutive checks. {check.ErrorMessage}";
        if (activeAlert is null)
        {
            dbContext.Alerts.Add(new AlertModel
            {
                Id = Guid.NewGuid(),
                MonitorId = monitor.Id,
                CreatedAt = check.CheckedAt,
                Status = AlertStatusModel.OPEN,
                Message = message,
                FailureCount = failureCount
            });
        }
        else
        {
            activeAlert.Message = message;
            activeAlert.FailureCount = failureCount;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
