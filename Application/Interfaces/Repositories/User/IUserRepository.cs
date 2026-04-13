using Application.Interfaces.Repositories.Common;
using Domain.Entities.User;
using Domain.Enums.User;

namespace Application.Interfaces.Repositories;

/// <summary>Queries for user entities.</summary>
public interface IUserRepository : IBaseRepository<UserEntity>
{
    /// <summary>Returns the user with the given email within a tenant, or <see langword="null"/>.</summary>
    Task<UserEntity?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the user with their sessions eagerly loaded, or <see langword="null"/>.</summary>
    Task<UserEntity?> GetByIdWithSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns <see langword="true"/> when the email is already taken within the tenant.</summary>
    Task<bool> ExistsByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all users belonging to the given tenant.</summary>
    Task<IReadOnlyList<UserEntity>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all users in the given status (cross-tenant, system-owner use only).</summary>
    Task<IReadOnlyList<UserEntity>> GetByStatusAsync(UserStatus status, CancellationToken cancellationToken = default);

    /// <summary>Returns all users with the given role within a tenant.</summary>
    Task<IReadOnlyList<UserEntity>> GetByRoleAsync(UserRole role, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the user whose password-reset token hash matches and has not expired, or <see langword="null"/>.</summary>
    Task<UserEntity?> GetByResetTokenHashAsync(string resetTokenHash, CancellationToken cancellationToken = default);

    /// <summary>Returns the user whose email-verification token hash matches and has not expired, or <see langword="null"/>.</summary>
    Task<UserEntity?> GetByEmailVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Returns the user whose pending-email token hash matches and has not expired, or <see langword="null"/>.</summary>
    Task<UserEntity?> GetByPendingEmailTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Returns a page of users ordered by email for the given tenant.</summary>
    Task<IReadOnlyList<UserEntity>> GetByTenantIdPagedAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Returns the total count of users belonging to the given tenant.</summary>
    Task<int> CountByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
