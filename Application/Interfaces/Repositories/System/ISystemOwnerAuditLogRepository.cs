using Domain.Entities.System;

namespace Application.Interfaces.Repositories.System;

public interface ISystemOwnerAuditLogRepository
{
    Task AddAsync(SystemOwnerAuditLogEntity entity, CancellationToken ct = default);
    Task<IReadOnlyList<SystemOwnerAuditLogEntity>> GetBySystemOwnerIdAsync(Guid systemOwnerId, int page, int pageSize, CancellationToken ct = default);
}
