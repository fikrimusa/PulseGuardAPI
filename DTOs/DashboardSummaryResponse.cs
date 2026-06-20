using PulseGuard.Api.Models;

namespace PulseGuard.Api.DTOs;

public sealed record DashboardSummaryResponse(
    int TotalMonitors,
    int EnabledMonitors,
    int DisabledMonitors,
    int UpMonitors,
    int DownMonitors,
    int UnknownMonitors,
    int OpenAlerts,
    int AcknowledgedAlerts,
    int ResolvedAlerts,
    double? AverageResponseTimeMs,
    IReadOnlyCollection<DashboardRecentAlertResponse> RecentAlerts,
    IReadOnlyCollection<DashboardRecentMonitorCheckResponse> RecentMonitorChecks);

public sealed record DashboardRecentAlertResponse(
    Guid Id,
    Guid MonitorId,
    string MonitorName,
    DateTime CreatedAt,
    AlertStatus Status,
    string Message,
    int FailureCount);

public sealed record DashboardRecentMonitorCheckResponse(
    Guid Id,
    Guid MonitorId,
    string MonitorName,
    DateTime CheckedAt,
    bool IsSuccess,
    int? StatusCode,
    long ResponseTimeMs,
    string? ErrorMessage);
