using Application.Interfaces.Repositories;
using Domain.Entities.User;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserSessionRepository : BaseRepository<UserSessionEntity>, IUserSessionRepository
{
    public UserSessionRepository(AppDbContext context) : base(context) { }

    public async Task<UserSessionEntity?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash && s.RefreshTokenRevokedAt == null && s.RefreshTokenExpiresAt > DateTimeOffset.UtcNow, cancellationToken);

    public async Task<IReadOnlyList<UserSessionEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().Where(s => s.UserId == userId).OrderByDescending(s => s.LastUsedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserSessionEntity>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().Where(s => s.UserId == userId && s.RefreshTokenRevokedAt == null && s.RefreshTokenExpiresAt > DateTimeOffset.UtcNow).OrderByDescending(s => s.LastUsedAt).ToListAsync(cancellationToken);

    public async Task RevokeAllByUserIdAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default)
        => await _dbSet.Where(s => s.UserId == userId && s.RefreshTokenRevokedAt == null).ExecuteUpdateAsync(s => s.SetProperty(x => x.RefreshTokenRevokedAt, revokedAt), cancellationToken);

    public async Task RevokeAllByTenantIdAsync(Guid tenantId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default)
        => await _dbSet.Where(s => s.TenantId == tenantId && s.RefreshTokenRevokedAt == null).ExecuteUpdateAsync(s => s.SetProperty(x => x.RefreshTokenRevokedAt, revokedAt), cancellationToken);
}