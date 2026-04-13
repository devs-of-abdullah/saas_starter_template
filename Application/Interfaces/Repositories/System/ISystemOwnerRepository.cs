using Domain.Entities.System;

namespace Application.Interfaces.Repositories.System;

public interface ISystemOwnerRepository
{
    Task<SystemOwnerEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SystemOwnerEntity?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<SystemOwnerEntity?> GetByResetTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(SystemOwnerEntity entity, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
}
