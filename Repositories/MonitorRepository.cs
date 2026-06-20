using Microsoft.EntityFrameworkCore;
using PulseGuard.Api.Data;
using MonitorModel = PulseGuard.Api.Models.Monitor;

namespace PulseGuard.Api.Repositories;

public sealed class MonitorRepository(AppDbContext dbContext)
{
    public IReadOnlyCollection<MonitorModel> GetAll(Guid userId)
    {
        return dbContext.Monitors
            .AsNoTracking()
            .Where(monitor => monitor.UserId == userId)
            .OrderByDescending(monitor => monitor.CreatedAtUtc)
            .ToArray();
    }

    public MonitorModel? GetById(Guid id, Guid userId)
    {
        return dbContext.Monitors
            .AsNoTracking()
            .SingleOrDefault(monitor => monitor.Id == id && monitor.UserId == userId);
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

    public bool Delete(Guid id, Guid userId)
    {
        var monitor = dbContext.Monitors.SingleOrDefault(monitor => monitor.Id == id && monitor.UserId == userId);
        if (monitor is null)
        {
            return false;
        }

        dbContext.Monitors.Remove(monitor);
        dbContext.SaveChanges();

        return true;
    }
}
