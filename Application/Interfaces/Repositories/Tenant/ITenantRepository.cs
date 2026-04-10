using Application.Interfaces.Repositories.Common;
using Domain.Entities.Tenant;
using Domain.Enums.Tenant;

namespace Application.Interfaces.Repositories.Tenant;

public interface ITenantRepository : IBaseRepository<TenantEntity>
{
    Task<TenantEntity?> GetByIdWithSettingsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantEntity>> GetByStatusAsync(TenantStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantEntity>> GetByPlanAsync(TenantPlan plan, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantEntity>> GetExpiredSubscriptionsAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default);
}