using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseGuard.Api.DTOs;
using PulseGuard.Api.Repositories;
using PulseGuard.Api.Services;
using MonitorModel = PulseGuard.Api.Models.Monitor;

namespace PulseGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/monitors")]
public sealed class MonitorsController(
    MonitorService monitorService,
    MonitorCheckRepository monitorCheckRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MonitorResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<MonitorResponse>> GetAll()
    {
        return Ok(monitorService.GetAll(GetUserId()).Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<MonitorResponse> GetById(Guid id)
    {
        var monitor = monitorService.GetById(id, GetUserId());

        return monitor is null ? NotFound() : Ok(ToResponse(monitor));
    }

    [HttpPost]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<MonitorResponse> Create(CreateMonitorRequest request)
    {
        var monitor = monitorService.Create(
            GetUserId(),
            request.Name,
            request.Url,
            request.CheckIntervalSeconds,
            request.TimeoutSeconds,
            request.ExpectedStatusCode,
            request.IsActive);

        var response = ToResponse(monitor);

        return CreatedAtAction(nameof(GetById), new { id = monitor.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<MonitorResponse> Update(Guid id, UpdateMonitorRequest request)
    {
        var monitor = monitorService.Update(
            id,
            GetUserId(),
            request.Name,
            request.Url,
            request.CheckIntervalSeconds,
            request.TimeoutSeconds,
            request.ExpectedStatusCode,
            request.IsActive);

        return monitor is null ? NotFound() : Ok(ToResponse(monitor));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        return monitorService.Delete(id, GetUserId()) ? NoContent() : NotFound();
    }

    [HttpGet("{id:guid}/checks")]
    [ProducesResponseType(typeof(IEnumerable<MonitorCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<MonitorCheckResponse>> GetChecks(Guid id)
    {
        var monitor = monitorService.GetById(id, GetUserId());
        if (monitor is null)
        {
            return NotFound();
        }

        var checks = monitorCheckRepository.GetForMonitor(id).Select(check => new MonitorCheckResponse(
            check.Id,
            check.MonitorId,
            check.CheckedAt,
            check.IsSuccess,
            check.StatusCode,
            check.ResponseTimeMs,
            check.ErrorMessage));

        return Ok(checks);
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private static MonitorResponse ToResponse(MonitorModel monitor)
    {
        return new MonitorResponse(
            monitor.Id,
            monitor.Name,
            monitor.Url,
            monitor.CheckIntervalSeconds,
            monitor.TimeoutSeconds,
            monitor.ExpectedStatusCode,
            monitor.IsActive,
            monitor.CreatedAtUtc,
            monitor.UpdatedAtUtc);
    }
}
