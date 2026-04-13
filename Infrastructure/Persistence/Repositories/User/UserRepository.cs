using Application.Interfaces.Repositories;
using Domain.Entities.User;
using Domain.Enums.User;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.User;

public sealed class UserRepository : BaseRepository<UserEntity>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

 
    public async Task<UserEntity?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default) => await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId, cancellationToken);

    public async Task<UserEntity?> GetByIdWithSessionsAsync(Guid userId, CancellationToken cancellationToken = default) => await _dbSet.Include(u => u.Sessions).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<UserEntity?> GetByResetTokenHashAsync(string resetTokenHash, CancellationToken cancellationToken = default)=> await _dbSet.FirstOrDefaultAsync(u => u.ResetTokenHash == resetTokenHash && u.ResetTokenExpiresAt > DateTimeOffset.UtcNow,cancellationToken);

    public async Task<UserEntity?> GetByEmailVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => await _dbSet.FirstOrDefaultAsync(u => u.EmailVerificationTokenHash == tokenHash && u.EmailVerificationTokenExpiresAt > DateTimeOffset.UtcNow,cancellationToken);


    public async Task<UserEntity?> GetByPendingEmailTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => await _dbSet.FirstOrDefaultAsync(u => u.PendingEmailTokenHash == tokenHash && u.PendingEmailTokenExpiresAt > DateTimeOffset.UtcNow,cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default)=> await _dbSet.AnyAsync(u => u.Email == email && u.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<UserEntity>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) => await _dbSet.AsNoTracking().Where(u => u.TenantId == tenantId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserEntity>> GetByTenantIdPagedAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)=> await _dbSet.AsNoTracking().Where(u => u.TenantId == tenantId).OrderBy(u => u.Email).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserEntity>> GetByStatusAsync(UserStatus status, CancellationToken cancellationToken = default) => await _dbSet.AsNoTracking().Where(u => u.Status == status).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserEntity>> GetByRoleAsync(UserRole role, Guid tenantId, CancellationToken cancellationToken = default) => await _dbSet.AsNoTracking().Where(u => u.Role == role && u.TenantId == tenantId).ToListAsync(cancellationToken);

    public async Task<int> CountByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) => await _dbSet.AsNoTracking().CountAsync(u => u.TenantId == tenantId, cancellationToken);
}
