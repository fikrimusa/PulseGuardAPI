using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseGuard.Api.DTOs;
using PulseGuard.Api.Services;

namespace PulseGuard.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<AuthResponse> Register(RegisterRequest request)
    {
        var response = authService.Register(request);

        return response is null
            ? Conflict(new { message = "An account with this email already exists." })
            : StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AuthResponse> Login(LoginRequest request)
    {
        var response = authService.Login(request);

        return response is null
            ? Unauthorized(new { message = "Invalid email or password." })
            : Ok(response);
    }
}
