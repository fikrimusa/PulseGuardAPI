namespace PulseGuard.Api.DTOs;

public sealed record MonitorResponse(
    Guid Id,
    string Name,
    string Url,
    int CheckIntervalSeconds,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
