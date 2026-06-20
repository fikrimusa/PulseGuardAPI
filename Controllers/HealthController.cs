using Microsoft.AspNetCore.Mvc;
using PulseGuard.Api.DTOs;

namespace PulseGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponseDto), StatusCodes.Status200OK)]
    public ActionResult<HealthResponseDto> Get()
    {
        var response = new HealthResponseDto(
            Status: "Healthy",
            TimestampUtc: DateTime.UtcNow);

        return Ok(response);
    }
}
