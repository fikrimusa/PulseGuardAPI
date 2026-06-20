using PulseGuard.Api.Models;

namespace PulseGuard.Api.DTOs;

public sealed record AlertResponse(
    Guid Id,
    Guid MonitorId,
    DateTime CreatedAt,
    DateTime? ResolvedAt,
    DateTime? AcknowledgedAt,
    AlertStatus Status,
    string Message,
    int FailureCount);
