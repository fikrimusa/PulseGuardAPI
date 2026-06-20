namespace PulseGuard.Api.DTOs;

public sealed record MonitorCheckResponse(
    Guid Id,
    Guid MonitorId,
    DateTime CheckedAt,
    bool IsSuccess,
    int? StatusCode,
    long ResponseTimeMs,
    string? ErrorMessage);
