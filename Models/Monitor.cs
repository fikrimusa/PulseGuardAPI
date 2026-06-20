namespace PulseGuard.Api.Models;

public sealed record Monitor(
    Guid Id,
    string Name,
    string Url,
    int CheckIntervalSeconds,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
