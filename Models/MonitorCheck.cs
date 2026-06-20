namespace PulseGuard.Api.Models;

public sealed class MonitorCheck
{
    public Guid Id { get; set; }

    public Guid MonitorId { get; set; }

    public Monitor Monitor { get; set; } = null!;

    public DateTime CheckedAt { get; set; }

    public bool IsSuccess { get; set; }

    public int? StatusCode { get; set; }

    public long ResponseTimeMs { get; set; }

    public string? ErrorMessage { get; set; }
}
