using Microsoft.EntityFrameworkCore;
using PulseGuard.Api.Data;
using AlertModel = PulseGuard.Api.Models.Alert;

namespace PulseGuard.Api.Repositories;

public sealed class AlertRepository(AppDbContext dbContext)
{
    public IReadOnlyCollection<AlertModel> GetAll(Guid userId)
    {
        return dbContext.Alerts
            .AsNoTracking()
            .Where(alert => alert.Monitor.UserId == userId)
            .OrderByDescending(alert => alert.CreatedAt)
            .ToArray();
    }

    public AlertModel? GetById(Guid id, Guid userId)
    {
        return dbContext.Alerts
            .SingleOrDefault(alert => alert.Id == id && alert.Monitor.UserId == userId);
    }

    public void SaveChanges()
    {
        dbContext.SaveChanges();
    }
}
