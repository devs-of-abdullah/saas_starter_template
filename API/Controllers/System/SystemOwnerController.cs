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
[Route("api/v{version:apiVersion}/system/me")]
[ApiController]
[Authorize(Policy = "SystemOwnerOnly")]
[EnableRateLimiting("GeneralLimiter")]
public sealed class SystemOwnerController : ControllerBase
{
    private readonly ISystemOwnerService _systemOwnerService;

    public SystemOwnerController(ISystemOwnerService systemOwnerService)
    {
        _systemOwnerService = systemOwnerService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SystemOwnerProfileDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        SystemOwnerProfileDTO profile = await _systemOwnerService.GetProfileAsync(GetSystemOwnerId(), ct);
        return Ok(ApiResponse<SystemOwnerProfileDTO>.Ok("Profile retrieved.", profile));
    }

    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SystemOwnerSessionDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions(CancellationToken ct)
    {
        IReadOnlyList<SystemOwnerSessionDTO> sessions =
            await _systemOwnerService.GetSessionsAsync(GetSystemOwnerId(), ct);
        return Ok(ApiResponse<IReadOnlyList<SystemOwnerSessionDTO>>.Ok("Sessions retrieved.", sessions));
    }

    [HttpDelete("sessions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(Guid id, CancellationToken ct)
    {
        await _systemOwnerService.RevokeSessionAsync(GetSystemOwnerId(), id, ct);
        return NoContent();
    }

    private Guid GetSystemOwnerId()
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("SystemOwner ID claim missing.");
        return Guid.Parse(value);
    }
}
