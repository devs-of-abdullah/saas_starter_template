using Domain.Entities.User;

namespace Application.Interfaces.Repositories;


public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntity entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogEntity>> GetByUserIdAsync(Guid userId, int page,int pageSize,CancellationToken ct = default);
}
