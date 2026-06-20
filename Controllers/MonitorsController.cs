using Microsoft.AspNetCore.Mvc;
using PulseGuard.Api.DTOs;
using PulseGuard.Api.Services;
using MonitorModel = PulseGuard.Api.Models.Monitor;

namespace PulseGuard.Api.Controllers;

[ApiController]
[Route("api/monitors")]
public sealed class MonitorsController(MonitorService monitorService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MonitorResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<MonitorResponse>> GetAll()
    {
        return Ok(monitorService.GetAll().Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<MonitorResponse> GetById(Guid id)
    {
        var monitor = monitorService.GetById(id);

        return monitor is null ? NotFound() : Ok(ToResponse(monitor));
    }

    [HttpPost]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<MonitorResponse> Create(CreateMonitorRequest request)
    {
        var monitor = monitorService.Create(
            request.Name,
            request.Url,
            request.CheckIntervalSeconds,
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
            request.Name,
            request.Url,
            request.CheckIntervalSeconds,
            request.IsActive);

        return monitor is null ? NotFound() : Ok(ToResponse(monitor));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        return monitorService.Delete(id) ? NoContent() : NotFound();
    }

    private static MonitorResponse ToResponse(MonitorModel monitor)
    {
        return new MonitorResponse(
            monitor.Id,
            monitor.Name,
            monitor.Url,
            monitor.CheckIntervalSeconds,
            monitor.IsActive,
            monitor.CreatedAtUtc,
            monitor.UpdatedAtUtc);
    }
}
