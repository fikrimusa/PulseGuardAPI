namespace PulseGuard.Api.Models;

public sealed class Monitor
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int CheckIntervalSeconds { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
