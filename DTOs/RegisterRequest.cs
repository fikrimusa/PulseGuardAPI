using System.ComponentModel.DataAnnotations;

namespace PulseGuard.Api.DTOs;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;
}
