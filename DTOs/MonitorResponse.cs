namespace PulseGuard.Api.DTOs;

public sealed record MonitorResponse(
    Guid Id,
    string Name,
    string Url,
    int CheckIntervalSeconds,
    int TimeoutSeconds,
    int ExpectedStatusCode,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
