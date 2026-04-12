using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Tenant;

namespace Application.Interfaces.Common;


public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    ITenantRepository Tenants { get; }
    IUserSessionRepository UserSessions { get; }
    IAuditLogRepository AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
