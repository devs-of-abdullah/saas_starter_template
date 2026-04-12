using Application.Interfaces.Repositories;
using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.User;

public sealed class AuditLogRepository : IAuditLogRepository
{
    readonly AppDbContext _context;
    readonly DbSet<AuditLogEntity> _dbSet;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<AuditLogEntity>();
    }

    public async Task AddAsync(AuditLogEntity entity, CancellationToken cancellationToken = default) => await _dbSet.AddAsync(entity, cancellationToken);
    public async Task<IReadOnlyList<AuditLogEntity>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default) => await _dbSet.AsNoTracking().Where(l => l.UserId == userId).OrderByDescending(l => l.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
}
