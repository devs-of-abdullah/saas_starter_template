using Application.Interfaces.Repositories.Tenant;

namespace Application.Interfaces.Repositories.Common;

public interface IUnitOfWork
{
    ITenantRepository Tenants { get; }
    ITenantSettingsRepository TenantSettings { get; }
    IUserRepository Users { get; }
    IUserSessionRepository UserSessions { get; }
    IAuditLogRepository AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}