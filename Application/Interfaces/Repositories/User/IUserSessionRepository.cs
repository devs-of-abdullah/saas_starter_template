using Application.Interfaces.Repositories.Common;
using Domain.Entities.User;

namespace Application.Interfaces.Repositories;

/// <summary>Queries for user-session entities.</summary>
public interface IUserSessionRepository : IBaseRepository<UserSessionEntity>
{
    /// <summary>
    /// Returns the session whose refresh-token hash matches, regardless of revocation or expiry status,
    /// so callers can distinguish token-theft (revoked) from simply-invalid (not found).
    /// </summary>
    Task<UserSessionEntity?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);

    /// <summary>Returns all sessions for the given user, ordered by last-used descending.</summary>
    Task<IReadOnlyList<UserSessionEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns only active (non-revoked, non-expired) sessions for the given user.</summary>
    Task<IReadOnlyList<UserSessionEntity>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Sets <c>RefreshTokenRevokedAt</c> on all active sessions for the given user.</summary>
    Task RevokeAllByUserIdAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default);

    /// <summary>Sets <c>RefreshTokenRevokedAt</c> on all active sessions for the given tenant.</summary>
    Task RevokeAllByTenantIdAsync(Guid tenantId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default);
}
