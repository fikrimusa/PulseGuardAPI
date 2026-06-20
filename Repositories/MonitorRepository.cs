using Microsoft.EntityFrameworkCore;
using PulseGuard.Api.Data;
using MonitorModel = PulseGuard.Api.Models.Monitor;

namespace PulseGuard.Api.Repositories;

public sealed class MonitorRepository(AppDbContext dbContext)
{
    public IReadOnlyCollection<MonitorModel> GetAll()
    {
        return dbContext.Monitors
            .AsNoTracking()
            .OrderByDescending(monitor => monitor.CreatedAtUtc)
            .ToArray();
    }

    public MonitorModel? GetById(Guid id)
    {
        return dbContext.Monitors
            .AsNoTracking()
            .SingleOrDefault(monitor => monitor.Id == id);
    }

    public MonitorModel Add(MonitorModel monitor)
    {
        dbContext.Monitors.Add(monitor);
        dbContext.SaveChanges();

        return monitor;
    }

    public MonitorModel Update(MonitorModel monitor)
    {
        dbContext.Monitors.Update(monitor);
        dbContext.SaveChanges();

        return monitor;
    }

    public bool Delete(Guid id)
    {
        var monitor = dbContext.Monitors.Find(id);
        if (monitor is null)
        {
            return false;
        }

        dbContext.Monitors.Remove(monitor);
        dbContext.SaveChanges();

        return true;
    }
}
