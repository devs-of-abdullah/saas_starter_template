using Application.Interfaces.Repositories.Common;
using Domain.Entities.Tenant;

namespace Application.Interfaces.Repositories.Tenant;

public interface ITenantSettingsRepository : IBaseRepository<TenantSettingsEntity>
{
    Task<TenantSettingsEntity?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantSettingsEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
