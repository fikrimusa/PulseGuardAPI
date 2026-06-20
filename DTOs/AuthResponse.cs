namespace PulseGuard.Api.DTOs;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string AccessToken,
    DateTime ExpiresAtUtc);
