using API.Models;
using Application.DTOs.Tenant;
using Application.Interfaces.System;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.System;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/system/tenants")]
[ApiController]
[EnableRateLimiting("GeneralLimiter")]
public sealed class TenantManagementController : ControllerBase
{
    private readonly ITenantManagementService _tenantManagementService;

    public TenantManagementController(ITenantManagementService tenantManagementService)
    {
        _tenantManagementService = tenantManagementService;
    }

    [HttpGet]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReadTenantDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        IReadOnlyList<ReadTenantDTO> tenants = await _tenantManagementService.GetAllAsync(page, pageSize, ct);
        return Ok(ApiResponse<IReadOnlyList<ReadTenantDTO>>.Ok("Tenants retrieved.", tenants));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(typeof(ApiResponse<ReadTenantDetailsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        ReadTenantDetailsDTO tenant = await _tenantManagementService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ReadTenantDetailsDTO>.Ok("Tenant retrieved.", tenant));
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ReadTenantDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        ReadTenantDTO tenant = await _tenantManagementService.GetBySlugAsync(slug, ct);
        return Ok(ApiResponse<ReadTenantDTO>.Ok("Tenant retrieved.", tenant));
    }

    [HttpPost]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(typeof(ApiResponse<ReadTenantDetailsDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateTenantDTO request, CancellationToken ct)
    {
        Guid tenantId = await _tenantManagementService.CreateAsync(request, GetSystemOwnerId(), ct);
        ReadTenantDetailsDTO tenant = await _tenantManagementService.GetByIdAsync(tenantId, ct);

        return CreatedAtAction(nameof(GetById), new { id = tenantId },
            ApiResponse<ReadTenantDetailsDTO>.Created("Tenant created.", tenant));
    }

    [HttpPut("{id:guid}/settings")]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSettings(
        Guid id, [FromBody] UpdateTenantSettingsDTO request, CancellationToken ct)
    {
        await _tenantManagementService.UpdateSettingsAsync(id, request, GetSystemOwnerId(), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/plan")]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdateTenantPlanDTO request, CancellationToken ct)
    {
        await _tenantManagementService.UpdatePlanAsync(id, request, GetSystemOwnerId(), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        await _tenantManagementService.SuspendAsync(id, GetSystemOwnerId(), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "SystemOwnerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _tenantManagementService.CancelAsync(id, GetSystemOwnerId(), ct);
        return NoContent();
    }

    private Guid GetSystemOwnerId()
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("SystemOwner ID claim missing.");
        return Guid.Parse(value);
    }
}
