using Application.Interfaces.Repositories.System;
using Domain.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.System;

public sealed class SystemOwnerAuditLogRepository : ISystemOwnerAuditLogRepository
{
    readonly AppDbContext _context;
    readonly DbSet<SystemOwnerAuditLogEntity> _dbSet;

    public SystemOwnerAuditLogRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<SystemOwnerAuditLogEntity>();
    }

    public async Task AddAsync(SystemOwnerAuditLogEntity entity, CancellationToken ct = default) =>
        await _dbSet.AddAsync(entity, ct);

    public async Task<IReadOnlyList<SystemOwnerAuditLogEntity>> GetBySystemOwnerIdAsync(Guid systemOwnerId, int page, int pageSize, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(a => a.SystemOwnerId == systemOwnerId).OrderByDescending(a => a.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
}
