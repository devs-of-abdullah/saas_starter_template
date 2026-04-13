using Application.DTOs.Tenant;

namespace Application.Interfaces.System;

public interface ITenantManagementService
{
    Task<IReadOnlyList<ReadTenantDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ReadTenantDetailsDTO> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<ReadTenantDTO> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Guid> CreateAsync(CreateTenantDTO dto, Guid systemOwnerId, CancellationToken ct = default);
    Task UpdateSettingsAsync(Guid tenantId, UpdateTenantSettingsDTO dto, Guid systemOwnerId, CancellationToken ct = default);
    Task UpdatePlanAsync(Guid tenantId, UpdateTenantPlanDTO dto, Guid systemOwnerId, CancellationToken ct = default);
    Task SuspendAsync(Guid tenantId, Guid systemOwnerId, CancellationToken ct = default);
    Task CancelAsync(Guid tenantId, Guid systemOwnerId, CancellationToken ct = default);
}
