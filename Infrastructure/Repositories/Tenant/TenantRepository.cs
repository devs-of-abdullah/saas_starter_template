using Application.Interfaces.Repositories.Tenant;
using Domain.Entities.Tenant;
using Domain.Enums.Tenant;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : BaseRepository<TenantEntity>, ITenantRepository
{
    public TenantRepository(AppDbContext context) : base(context) { }

    public async Task<TenantEntity?> GetByIdWithSettingsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await _dbSet.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

    public async Task<IReadOnlyList<TenantEntity>> GetByStatusAsync(TenantStatus status, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().Where(t => t.Status == status).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantEntity>> GetByPlanAsync(TenantPlan plan, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().Where(t => t.Plan == plan).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantEntity>> GetExpiredSubscriptionsAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().Where(t => t.SubscriptionEndsAt.HasValue && t.SubscriptionEndsAt.Value <= asOf).ToListAsync(cancellationToken);
}