using Application.Interfaces.Repositories.Tenant;
using Domain.Entities.Tenant;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Tenant;

public sealed class TenantSettingsRepository : BaseRepository<TenantSettingsEntity>, ITenantSettingsRepository
{
    public TenantSettingsRepository(AppDbContext context) : base(context) { }

    public async Task<TenantSettingsEntity?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)=> await _dbSet.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

    public async Task<TenantSettingsEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) => await _dbSet.AsNoTracking().FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default) => await _dbSet.AnyAsync(s => s.Slug == slug, cancellationToken);
}
