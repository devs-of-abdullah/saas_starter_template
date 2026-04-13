using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.System;
using Application.Interfaces.Repositories.Tenant;
using Infrastructure.Persistence.Repositories.System;
using Infrastructure.Persistence.Repositories.Tenant;
using Infrastructure.Persistence.Repositories.User;

namespace Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    readonly AppDbContext _context;

    IUserRepository?                _users;
    ITenantRepository?              _tenants;
    IUserSessionRepository?         _userSessions;
    IAuditLogRepository?            _auditLogs;
    ISystemOwnerRepository?         _systemOwners;
    ISystemOwnerSessionRepository?  _systemOwnerSessions;
    ISystemOwnerAuditLogRepository? _systemOwnerAuditLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);
    public IUserSessionRepository UserSessions => _userSessions ??= new UserSessionRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
    public ISystemOwnerRepository SystemOwners => _systemOwners ??= new SystemOwnerRepository(_context);
    public ISystemOwnerSessionRepository SystemOwnerSessions  => _systemOwnerSessions  ??= new SystemOwnerSessionRepository(_context);
    public ISystemOwnerAuditLogRepository SystemOwnerAuditLogs => _systemOwnerAuditLogs ??= new SystemOwnerAuditLogRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}
