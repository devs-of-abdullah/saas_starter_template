using Application.Interfaces.Repositories;
using Domain.Entities.User;
using Domain.Enums.User;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(AuditLogEntity auditLog, CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AddAsync(auditLog, cancellationToken);

    public async Task<IReadOnlyList<AuditLogEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AsNoTracking().Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLogEntity>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AsNoTracking().Where(a => a.TenantId == tenantId).OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLogEntity>> GetByActionAsync(AuditAction action, Guid tenantId, CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AsNoTracking().Where(a => a.Action == action && a.TenantId == tenantId).OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
}