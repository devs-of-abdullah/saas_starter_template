using API.Models;
using Application.Constants;
using Application.DTOs.User;
using Application.Interfaces.Services.User;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.User;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[ApiController]
[Authorize]
[EnableRateLimiting("GeneralLimiter")]
public sealed class UsersController : ControllerBase
{
    readonly IUserService _userService;
    readonly IAuthorizationService _authorizationService;

    public UsersController(IUserService userService, IAuthorizationService authorizationService)
    {
        _userService = userService;
        _authorizationService = authorizationService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ReadUserDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        AuthorizationResult authResult =
            await _authorizationService.AuthorizeAsync(User, id, ClaimConstants.PolicyUserOwnerOrAdmin);
        if (!authResult.Succeeded)
            return Forbid();

        ReadUserDTO user = await _userService.GetByIdAsync(id, GetTenantId(), ct);
        return Ok(new ApiResponse<ReadUserDTO>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "User retrieved.",
            Data = user
        });
    }

    [HttpGet]
    [Authorize(Roles = "TenantSuperAdmin,TenantAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReadUserDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        IReadOnlyList<ReadUserDTO> users = await _userService.GetAllByTenantAsync(GetTenantId(), page, pageSize, ct);
        return Ok(new ApiResponse<IReadOnlyList<ReadUserDTO>>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Users retrieved.",
            Data = users
        });
    }

    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdateUserPasswordDTO request, CancellationToken ct)
    {
        (Guid userId, Guid tenantId) = GetUserContext();
        await _userService.UpdatePasswordAsync(userId, tenantId, request, ct);
        return NoContent();
    }

    [HttpPut("me/email")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateUserEmailDTO request, CancellationToken ct)
    {
        (Guid userId, Guid tenantId) = GetUserContext();
        await _userService.UpdateEmailAsync(userId, tenantId, request, ct);
        return Ok(ApiResponse.Ok("A confirmation code has been sent to your new email address."));
    }


    [HttpPost("me/email/confirm")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequestDTO request, CancellationToken ct)
    {
        bool success = await _userService.ConfirmEmailChangeAsync(request.Code, ct);
        if (!success)
            return BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest, "Invalid or expired confirmation code."));

        return Ok(ApiResponse.Ok("Email updated successfully. Please log in again."));
    }

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMe([FromBody] DeleteUserDTO request, CancellationToken ct)
    {
        (Guid userId, Guid tenantId) = GetUserContext();
        await _userService.DeleteAsync(userId, tenantId, request, ct);
        return NoContent();
    }

 
    [HttpPatch("{id:guid}/role")]
    [Authorize(Roles = "TenantSuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleDTO request, CancellationToken ct)
    {
        await _userService.UpdateRoleAsync(id, GetTenantId(), request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "TenantSuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdminDeleteUser(Guid id, CancellationToken ct)
    {
        await _userService.AdminDeleteAsync(id, GetTenantId(), ct);
        return NoContent();
    }


    Guid GetUserId()
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID claim missing from token.");
        return Guid.Parse(value);
    }

    Guid GetTenantId()
    {
        string? value = User.FindFirstValue(ClaimConstants.TenantId) ?? throw new UnauthorizedAccessException("Tenant ID claim missing from token.");
        return Guid.Parse(value);
    }

    (Guid userId, Guid tenantId) GetUserContext() => (GetUserId(), GetTenantId());
}
