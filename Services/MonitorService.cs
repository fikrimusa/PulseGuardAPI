using System.Collections.Concurrent;
using MonitorModel = PulseGuard.Api.Models.Monitor;

namespace PulseGuard.Api.Services;

public sealed class MonitorService
{
    private readonly ConcurrentDictionary<Guid, MonitorModel> _monitors = new();

    public IReadOnlyCollection<MonitorModel> GetAll()
    {
        return _monitors.Values
            .OrderByDescending(monitor => monitor.CreatedAtUtc)
            .ToArray();
    }

    public MonitorModel? GetById(Guid id)
    {
        return _monitors.GetValueOrDefault(id);
    }

    public MonitorModel Create(string name, string url, int checkIntervalSeconds, bool isActive)
    {
        var now = DateTime.UtcNow;
        var monitor = new MonitorModel(
            Id: Guid.NewGuid(),
            Name: name.Trim(),
            Url: url.Trim(),
            CheckIntervalSeconds: checkIntervalSeconds,
            IsActive: isActive,
            CreatedAtUtc: now,
            UpdatedAtUtc: now);

        _monitors[monitor.Id] = monitor;

        return monitor;
    }

    public MonitorModel? Update(Guid id, string name, string url, int checkIntervalSeconds, bool isActive)
    {
        while (_monitors.TryGetValue(id, out var current))
        {
            var updated = current with
            {
                Name = name.Trim(),
                Url = url.Trim(),
                CheckIntervalSeconds = checkIntervalSeconds,
                IsActive = isActive,
                UpdatedAtUtc = DateTime.UtcNow
            };

            if (_monitors.TryUpdate(id, updated, current))
            {
                return updated;
            }
        }

        return null;
    }

    public bool Delete(Guid id)
    {
        return _monitors.TryRemove(id, out _);
    }
}
