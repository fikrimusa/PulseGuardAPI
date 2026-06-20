using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PulseGuard.Api.Configuration;
using PulseGuard.Api.Data;
using MonitorModel = PulseGuard.Api.Models.Monitor;
using MonitorCheckModel = PulseGuard.Api.Models.MonitorCheck;

namespace PulseGuard.Api.Services;

public sealed class HealthCheckWorker(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<HealthCheckWorkerSettings> settings,
    ILogger<HealthCheckWorker> logger) : BackgroundService
{
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(Math.Max(1, settings.Value.PollingIntervalSeconds));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunDueChecksAsync(stoppingToken);
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task RunDueChecksAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var monitors = await dbContext.Monitors
                .AsNoTracking()
                .Where(monitor => monitor.IsActive)
                .ToListAsync(stoppingToken);

            if (monitors.Count == 0)
            {
                return;
            }

            var monitorIds = monitors.Select(monitor => monitor.Id).ToArray();
            var latestChecks = await dbContext.MonitorChecks
                .AsNoTracking()
                .Where(check => monitorIds.Contains(check.MonitorId))
                .GroupBy(check => check.MonitorId)
                .Select(group => new { MonitorId = group.Key, CheckedAt = group.Max(check => check.CheckedAt) })
                .ToDictionaryAsync(check => check.MonitorId, check => check.CheckedAt, stoppingToken);

            var now = DateTime.UtcNow;
            foreach (var monitor in monitors.Where(monitor =>
                         !latestChecks.TryGetValue(monitor.Id, out var lastCheck)
                         || now - lastCheck >= TimeSpan.FromSeconds(monitor.CheckIntervalSeconds)))
            {
                await CheckMonitorAsync(dbContext, monitor, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unexpected error occurred while running monitor health checks.");
        }
    }

    private async Task CheckMonitorAsync(AppDbContext dbContext, MonitorModel monitor, CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();
        int? statusCode = null;
        var isSuccess = false;
        string? errorMessage = null;

        try
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(monitor.TimeoutSeconds));

            using var request = new HttpRequestMessage(HttpMethod.Get, monitor.Url);
            using var response = await httpClientFactory.CreateClient("MonitorChecks")
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutSource.Token);

            statusCode = (int)response.StatusCode;
            isSuccess = statusCode == monitor.ExpectedStatusCode;
            if (!isSuccess)
            {
                errorMessage = $"Expected HTTP {monitor.ExpectedStatusCode} but received HTTP {statusCode}.";
            }
        }
        catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
        {
            errorMessage = $"Request timed out after {monitor.TimeoutSeconds} seconds.";
        }
        catch (HttpRequestException exception)
        {
            errorMessage = exception.Message;
        }
        catch (Exception exception)
        {
            errorMessage = exception.Message;
            logger.LogWarning(exception, "Health check failed unexpectedly for monitor {MonitorId}.", monitor.Id);
        }
        finally
        {
            stopwatch.Stop();

            dbContext.MonitorChecks.Add(new MonitorCheckModel
            {
                Id = Guid.NewGuid(),
                MonitorId = monitor.Id,
                CheckedAt = DateTime.UtcNow,
                IsSuccess = isSuccess,
                StatusCode = statusCode,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorMessage = errorMessage
            });

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
