using Application.Interfaces.Repositories.System;
using Domain.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.System;

public sealed class SystemOwnerRepository : ISystemOwnerRepository
{
    readonly AppDbContext _context;
    readonly DbSet<SystemOwnerEntity> _dbSet;

    public SystemOwnerRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<SystemOwnerEntity>();
    }

    public async Task<SystemOwnerEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _dbSet.FindAsync([id], ct);

    public async Task<SystemOwnerEntity?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(o => o.Email == email, ct);

    public async Task<SystemOwnerEntity?> GetByResetTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(
            o => o.ResetTokenHash == tokenHash && o.ResetTokenExpiresAt > DateTimeOffset.UtcNow, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(o => o.Email == email, ct);

    public async Task AddAsync(SystemOwnerEntity entity, CancellationToken ct = default) =>
        await _dbSet.AddAsync(entity, ct);

    public async Task<bool> AnyAsync(CancellationToken ct = default) =>
        await _dbSet.AnyAsync(ct);
}
