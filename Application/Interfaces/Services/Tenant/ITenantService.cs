using Application.DTOs.Tenant;

namespace Application.Interfaces.Services.Tenant;

/// <summary>Manages tenant lifecycle, settings, and subscription plans.</summary>
public interface ITenantService
{
    /// <summary>Get a tenant by ID. Visible to authenticated users in that tenant.</summary>
    Task<ReadTenantDTO> GetByIdAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>Get a tenant by slug. Public endpoint — no auth required.</summary>
    Task<ReadTenantDTO> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Get tenant settings. TenantSuperAdmin or SystemOwner only.</summary>
    Task<ReadTenantSettingsDTO> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>Get all tenants paginated. SystemOwner only.</summary>
    Task<IReadOnlyList<ReadTenantDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Create a new tenant. SystemOwner only.</summary>
    Task<Guid> CreateAsync(CreateTenantDTO dto, CancellationToken ct = default);

    /// <summary>Update tenant settings. TenantSuperAdmin or SystemOwner only.</summary>
    Task UpdateSettingsAsync(Guid tenantId, UpdateTenantSettingsDTO dto, CancellationToken ct = default);

    /// <summary>Update tenant subscription plan. SystemOwner only.</summary>
    Task UpdatePlanAsync(Guid tenantId, UpdateTenantPlanDTO dto, CancellationToken ct = default);

    /// <summary>Suspend a tenant — users cannot log in. SystemOwner only.</summary>
    Task SuspendAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>Cancel a tenant. SystemOwner only.</summary>
    Task CancelAsync(Guid tenantId, CancellationToken ct = default);
}
