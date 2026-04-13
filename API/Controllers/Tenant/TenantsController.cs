using API.Models;
using Application.Constants;
using Application.DTOs.Tenant;
using Application.Interfaces.Services.Tenant;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Tenant;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tenants")]
[ApiController]
[EnableRateLimiting("GeneralLimiter")]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>Get a tenant by slug (public endpoint — used by frontends to resolve a tenant).</summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ReadTenantDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        ReadTenantDTO tenant = await _tenantService.GetBySlugAsync(slug, ct);
        return Ok(ApiResponse<ReadTenantDTO>.Ok("Tenant retrieved.", tenant));
    }

    /// <summary>Get a tenant by ID (authenticated members of that tenant only).</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ReadTenantDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        string? tenantIdClaim = User.FindFirstValue(ClaimConstants.TenantId);
        if (tenantIdClaim != id.ToString())
            return Forbid();

        ReadTenantDTO tenant = await _tenantService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ReadTenantDTO>.Ok("Tenant retrieved.", tenant));
    }

    /// <summary>Get tenant settings (TenantSuperAdmin only).</summary>
    [HttpGet("{id:guid}/settings")]
    [Authorize(Roles = "TenantSuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ReadTenantSettingsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettings(Guid id, CancellationToken ct)
    {
        string? tenantIdClaim = User.FindFirstValue(ClaimConstants.TenantId);
        if (tenantIdClaim != id.ToString())
            return Forbid();

        ReadTenantSettingsDTO settings = await _tenantService.GetSettingsAsync(id, ct);
        return Ok(ApiResponse<ReadTenantSettingsDTO>.Ok("Tenant settings retrieved.", settings));
    }

    /// <summary>Update tenant settings (TenantSuperAdmin only).</summary>
    [HttpPut("{id:guid}/settings")]
    [Authorize(Roles = "TenantSuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSettings(Guid id, [FromBody] UpdateTenantSettingsDTO request, CancellationToken ct)
    {
        string? tenantIdClaim = User.FindFirstValue(ClaimConstants.TenantId);
        if (tenantIdClaim != id.ToString())
            return Forbid();

        await _tenantService.UpdateSettingsAsync(id, request, ct);
        return NoContent();
    }
}
