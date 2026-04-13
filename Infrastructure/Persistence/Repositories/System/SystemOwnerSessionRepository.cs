using Application.Interfaces.Repositories.System;
using Domain.Entities.System;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.System;

public sealed class SystemOwnerSessionRepository : BaseRepository<SystemOwnerSessionEntity>, ISystemOwnerSessionRepository
{
    public SystemOwnerSessionRepository(AppDbContext context) : base(context) { }

    public async Task<SystemOwnerSessionEntity?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(s => s.RefreshTokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<SystemOwnerSessionEntity>> GetBySystemOwnerIdAsync(Guid systemOwnerId, CancellationToken ct = default) =>
        await _dbSet.AsNoTracking().Where(s => s.SystemOwnerId == systemOwnerId).OrderByDescending(s => s.LastUsedAt).ToListAsync(ct);

    public async Task RevokeAllBySystemOwnerIdAsync(Guid systemOwnerId, DateTimeOffset revokedAt, CancellationToken ct = default) =>
        await _dbSet.Where(s => s.SystemOwnerId == systemOwnerId && s.RefreshTokenRevokedAt == null).ExecuteUpdateAsync(s => s.SetProperty(x => x.RefreshTokenRevokedAt, revokedAt), ct);
}
