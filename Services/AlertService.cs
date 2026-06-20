using PulseGuard.Api.Models;
using PulseGuard.Api.Repositories;
using AlertModel = PulseGuard.Api.Models.Alert;

namespace PulseGuard.Api.Services;

public sealed class AlertService(AlertRepository alertRepository)
{
    public IReadOnlyCollection<AlertModel> GetAll(Guid userId)
    {
        return alertRepository.GetAll(userId);
    }

    public AlertModel? GetById(Guid id, Guid userId)
    {
        return alertRepository.GetById(id, userId);
    }

    public AlertModel? Acknowledge(Guid id, Guid userId)
    {
        var alert = alertRepository.GetById(id, userId);
        if (alert is null || alert.Status == AlertStatus.RESOLVED)
        {
            return null;
        }

        if (alert.Status == AlertStatus.OPEN)
        {
            alert.Status = AlertStatus.ACKNOWLEDGED;
            alert.AcknowledgedAt = DateTime.UtcNow;
            alertRepository.SaveChanges();
        }

        return alert;
    }
}
