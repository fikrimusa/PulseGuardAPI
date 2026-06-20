using System.ComponentModel.DataAnnotations;

namespace PulseGuard.Api.DTOs;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;
}
