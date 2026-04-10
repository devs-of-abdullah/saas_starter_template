using Domain.Entities.User;
using Domain.Enums.User;

namespace Application.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntity auditLog, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogEntity>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogEntity>> GetByActionAsync(AuditAction action, Guid tenantId, CancellationToken cancellationToken = default);
}