using Application.Interfaces.Repositories.Tenant;
using Domain.Entities.Tenant;
using Domain.Enums.Tenant;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Tenant;

public sealed class TenantRepository : BaseRepository<TenantEntity>, ITenantRepository
{
    public TenantRepository(AppDbContext context) : base(context) { }

    public async Task<TenantEntity?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _dbSet.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Settings != null && t.Settings.Slug == slug, ct);

    public async Task<TenantEntity?> GetByIdWithSettingsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _dbSet.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

    public async Task<IReadOnlyList<TenantEntity>> GetAllPagedAsync(int page, int pageSize, CancellationToken ct = default) =>
        await _dbSet.AsNoTracking().OrderBy(t => t.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

    public async Task<IReadOnlyList<TenantEntity>> GetByStatusAsync(TenantStatus status, CancellationToken cancellationToken = default) =>
        await _dbSet.AsNoTracking().Where(t => t.Status == status).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantEntity>> GetByPlanAsync(TenantPlan plan, CancellationToken cancellationToken = default) =>
        await _dbSet.AsNoTracking().Where(t => t.Plan == plan).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantEntity>> GetExpiredSubscriptionsAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default) =>
        await _dbSet.AsNoTracking().Where(t => t.SubscriptionEndsAt.HasValue && t.SubscriptionEndsAt.Value <= asOf).ToListAsync(cancellationToken);

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(t => t.Settings != null && t.Settings.Slug == slug, ct);

    public async Task<IReadOnlyList<TenantEntity>> GetAllPagedWithSettingsAsync(int page, int pageSize, CancellationToken ct = default) =>
        await _dbSet
            .AsNoTracking()
            .Include(t => t.Settings)
            .OrderBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
}
