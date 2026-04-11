using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Tenant;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IUserRepository? _users;
    private ITenantRepository? _tenants;
    private IUserSessionRepository? _userSessions;
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);
    public IUserSessionRepository UserSessions => _userSessions ??= new UserSessionRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async ValueTask DisposeAsync()  => await _context.DisposeAsync();
}