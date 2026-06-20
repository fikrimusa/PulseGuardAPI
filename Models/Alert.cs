namespace PulseGuard.Api.Models;

public sealed class Alert
{
    public Guid Id { get; set; }

    public Guid MonitorId { get; set; }

    public Monitor Monitor { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    public AlertStatus Status { get; set; }

    public string Message { get; set; } = string.Empty;

    public int FailureCount { get; set; }
}
