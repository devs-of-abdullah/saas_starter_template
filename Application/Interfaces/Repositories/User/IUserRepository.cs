using Application.Interfaces.Repositories.Common;
using Domain.Entities.User;
using Domain.Enums.User;

namespace Application.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<UserEntity>
{
    Task<UserEntity?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByIdWithSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserEntity>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserEntity>> GetByStatusAsync(UserStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserEntity>> GetByRoleAsync(UserRole role, Guid tenantId, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByResetTokenHashAsync(string resetTokenHash, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByEmailVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}