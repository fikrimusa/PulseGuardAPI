using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseGuard.Api.DTOs;
using PulseGuard.Api.Services;
using AlertModel = PulseGuard.Api.Models.Alert;

namespace PulseGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerts")]
public sealed class AlertsController(AlertService alertService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlertResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<AlertResponse>> GetAll()
    {
        return Ok(alertService.GetAll(GetUserId()).Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AlertResponse> GetById(Guid id)
    {
        var alert = alertService.GetById(id, GetUserId());

        return alert is null ? NotFound() : Ok(ToResponse(alert));
    }

    [HttpPut("{id:guid}/acknowledge")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AlertResponse> Acknowledge(Guid id)
    {
        var alert = alertService.Acknowledge(id, GetUserId());

        return alert is null ? NotFound() : Ok(ToResponse(alert));
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private static AlertResponse ToResponse(AlertModel alert)
    {
        return new AlertResponse(
            alert.Id,
            alert.MonitorId,
            alert.CreatedAt,
            alert.ResolvedAt,
            alert.AcknowledgedAt,
            alert.Status,
            alert.Message,
            alert.FailureCount);
    }
}
