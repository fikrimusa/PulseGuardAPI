namespace PulseGuard.Api.Configuration;

public sealed class HealthCheckWorkerSettings
{
    public const string SectionName = "HealthCheckWorker";

    public int PollingIntervalSeconds { get; set; } = 10;
}
