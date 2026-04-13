using API.Models;
using Application.DTOs.System;
using Application.Interfaces.System;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.System;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/system/auth")]
[ApiController]
[EnableRateLimiting("AuthLimiter")]
public sealed class SystemOwnerAuthController : ControllerBase
{
    private readonly ISystemOwnerAuthService _authService;

    public SystemOwnerAuthController(ISystemOwnerAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SystemOwnerTokenResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] SystemOwnerLoginRequestDTO request, CancellationToken ct)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string? userAgent = Request.Headers.UserAgent.ToString();
        string? deviceInfo = Request.Headers["X-Device-Info"].ToString();

        SystemOwnerTokenResponseDTO token = await _authService.LoginAsync(
            request, ipAddress, userAgent, deviceInfo, ct);

        return Ok(ApiResponse<SystemOwnerTokenResponseDTO>.Ok("Login successful.", token));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SystemOwnerTokenResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] SystemOwnerRefreshRequestDTO request, CancellationToken ct)
    {
        SystemOwnerTokenResponseDTO token = await _authService.RefreshAsync(request, ct);
        return Ok(ApiResponse<SystemOwnerTokenResponseDTO>.Ok("Token refreshed.", token));
    }

    [HttpPost("logout")]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] SystemOwnerLogoutRequestDTO request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] SystemOwnerForgotPasswordRequestDTO request, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request, ct);
        return Ok(ApiResponse.Ok("If that email is registered, a reset code has been sent."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] SystemOwnerResetPasswordRequestDTO request, CancellationToken ct)
    {
        bool success = await _authService.ResetPasswordAsync(request, ct);

        if (!success)
            return BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest,
                "Invalid or expired reset code."));

        return Ok(ApiResponse.Ok("Password reset successfully."));
    }

    [HttpPut("password")]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] SystemOwnerChangePasswordRequestDTO request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(GetSystemOwnerId(), request, ct);
        return NoContent();
    }

    private Guid GetSystemOwnerId()
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("SystemOwner ID claim missing.");
        return Guid.Parse(value);
    }
}
