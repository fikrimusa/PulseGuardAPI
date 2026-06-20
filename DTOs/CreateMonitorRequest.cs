using System.ComponentModel.DataAnnotations;

namespace PulseGuard.Api.DTOs;

public sealed class CreateMonitorRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [Url]
    [StringLength(2048)]
    public string Url { get; init; } = string.Empty;

    [Range(10, 86_400)]
    public int CheckIntervalSeconds { get; init; } = 60;

    [Range(1, 120)]
    public int TimeoutSeconds { get; init; } = 10;

    [Range(100, 599)]
    public int ExpectedStatusCode { get; init; } = 200;

    public bool IsActive { get; init; } = true;
}
