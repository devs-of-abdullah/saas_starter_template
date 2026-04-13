using Application.Interfaces.Repositories.Common;
using Domain.Entities.System;

namespace Application.Interfaces.Repositories.System;

public interface ISystemOwnerSessionRepository : IBaseRepository<SystemOwnerSessionEntity>
{
    /// <summary>Returns the session matching the hash regardless of revocation status — required for theft detection.</summary>
    Task<SystemOwnerSessionEntity?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default);

    Task<IReadOnlyList<SystemOwnerSessionEntity>> GetBySystemOwnerIdAsync(Guid systemOwnerId, CancellationToken ct = default);

    Task RevokeAllBySystemOwnerIdAsync(Guid systemOwnerId, DateTimeOffset revokedAt, CancellationToken ct = default);
}
