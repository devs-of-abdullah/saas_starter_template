using Application.Interfaces.Repositories.Common;
using Domain.Entities.Tenant;
using Domain.Enums.Tenant;

namespace Application.Interfaces.Repositories.Tenant;

public interface ITenantRepository : IBaseRepository<TenantEntity>
{
    Task<TenantEntity?> GetByIdWithSettingsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantEntity?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<IReadOnlyList<TenantEntity>> GetAllPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns a page of tenants with their settings eagerly loaded.</summary>
    Task<IReadOnlyList<TenantEntity>> GetAllPagedWithSettingsAsync(int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<TenantEntity>> GetByStatusAsync(TenantStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantEntity>> GetByPlanAsync(TenantPlan plan, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantEntity>> GetExpiredSubscriptionsAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
}
