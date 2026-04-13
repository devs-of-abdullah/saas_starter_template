using Application.Interfaces.Services.User;
using Domain.Entities.Common;
using Domain.Entities.System;
using Domain.Entities.Tenant;
using Domain.Entities.User;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    readonly ICurrentUserService _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.LazyLoadingEnabled = false;
        
    }

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<TenantSettingsEntity> TenantSettings => Set<TenantSettingsEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<UserSessionEntity> UserSessions => Set<UserSessionEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();
    public DbSet<SystemOwnerEntity> SystemOwners => Set<SystemOwnerEntity>();
    public DbSet<SystemOwnerSessionEntity> SystemOwnerSessions => Set<SystemOwnerSessionEntity>();
    public DbSet<SystemOwnerAuditLogEntity> SystemOwnerAuditLogs => Set<SystemOwnerAuditLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); //No registration needed. No manual calls  this register all configurations
        ApplyGlobalQueryFilters(modelBuilder);
    }

    void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantSettingsEntity>().HasQueryFilter(s => !_currentUser.IsAuthenticated || _currentUser.IsSystemOwner || s.TenantId == _currentUser.TenantId);

        modelBuilder.Entity<UserEntity>().HasQueryFilter(u => !_currentUser.IsAuthenticated || _currentUser.IsSystemOwner || u.TenantId == _currentUser.TenantId);

        modelBuilder.Entity<UserSessionEntity>().HasQueryFilter(s => !_currentUser.IsAuthenticated || _currentUser.IsSystemOwner || s.TenantId == _currentUser.TenantId);

        modelBuilder.Entity<AuditLogEntity>().HasQueryFilter(a => !_currentUser.IsAuthenticated || _currentUser.IsSystemOwner || a.TenantId == _currentUser.TenantId);
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    void ApplyAuditFields()
    {
        ChangeTracker.DetectChanges();

        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<ImmutableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                continue;
            }

            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new ImmutableEntityException(entry.Entity.GetType().Name);
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted when entry.Entity is TenantEntity or UserEntity:
                    throw new InvalidOperationException($"Hard delete is not allowed for {entry.Entity.GetType().Name}. Use status fields instead.");
            }
        }
    }
}