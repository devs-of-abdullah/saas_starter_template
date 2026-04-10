using Application.Interfaces.Repositories.Common;
using Domain.Entities.User;

namespace Application.Interfaces.Repositories;

public interface IUserSessionRepository : IBaseRepository<UserSessionEntity>
{
    Task<UserSessionEntity?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSessionEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSessionEntity>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAllByUserIdAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default);
    Task RevokeAllByTenantIdAsync(Guid tenantId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default);
}