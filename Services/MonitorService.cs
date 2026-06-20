using PulseGuard.Api.Repositories;
using MonitorModel = PulseGuard.Api.Models.Monitor;

namespace PulseGuard.Api.Services;

public sealed class MonitorService(MonitorRepository monitorRepository)
{
    public IReadOnlyCollection<MonitorModel> GetAll()
    {
        return monitorRepository.GetAll();
    }

    public MonitorModel? GetById(Guid id)
    {
        return monitorRepository.GetById(id);
    }

    public MonitorModel Create(string name, string url, int checkIntervalSeconds, bool isActive)
    {
        var now = DateTime.UtcNow;
        var monitor = new MonitorModel
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Url = url.Trim(),
            CheckIntervalSeconds = checkIntervalSeconds,
            IsActive = isActive,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        return monitorRepository.Add(monitor);
    }

    public MonitorModel? Update(Guid id, string name, string url, int checkIntervalSeconds, bool isActive)
    {
        var monitor = monitorRepository.GetById(id);
        if (monitor is null)
        {
            return null;
        }

        monitor.Name = name.Trim();
        monitor.Url = url.Trim();
        monitor.CheckIntervalSeconds = checkIntervalSeconds;
        monitor.IsActive = isActive;
        monitor.UpdatedAtUtc = DateTime.UtcNow;

        return monitorRepository.Update(monitor);
    }

    public bool Delete(Guid id)
    {
        return monitorRepository.Delete(id);
    }
}
