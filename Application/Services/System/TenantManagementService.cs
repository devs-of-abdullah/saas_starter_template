using Application.DTOs.Tenant;
using Application.Interfaces.Common;
using Application.Interfaces.System;
using Domain.Entities.System;
using Domain.Entities.Tenant;
using Domain.Enums.System;
using Domain.Enums.Tenant;
using Domain.Exceptions;
using ValidationException = Domain.Exceptions.ValidationException;

namespace Application.Services.System;

public sealed class TenantManagementService : ITenantManagementService
{
    private readonly IUnitOfWork _uow;

    public TenantManagementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<ReadTenantDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1)
            throw new ValidationException(nameof(page), "Page must be 1 or greater.");

        if (pageSize < 1 || pageSize > 100)
            throw new ValidationException(nameof(pageSize), "Page size must be between 1 and 100.");

        IReadOnlyList<TenantEntity> tenants = await _uow.Tenants.GetAllPagedWithSettingsAsync(page, pageSize, ct);
        return tenants.Select(MapToReadTenantDTO).ToList();
    }

    public async Task<ReadTenantDetailsDTO> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantWithSettingsOrThrowAsync(tenantId, ct);
        int userCount = await _uow.Users.CountByTenantIdAsync(tenantId, ct);
        return MapToDetailsDTO(tenant, userCount);
    }

    public async Task<ReadTenantDTO> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        TenantEntity? tenant = await _uow.Tenants.GetBySlugAsync(normalizedSlug, ct);

        if (tenant is null)
            throw new NotFoundException($"Tenant with slug '{normalizedSlug}' not found.");

        return MapToReadTenantDTO(tenant);
    }

    public async Task<Guid> CreateAsync(CreateTenantDTO dto, Guid systemOwnerId, CancellationToken ct = default)
    {
        string normalizedSlug = dto.Slug.Trim().ToLowerInvariant();

        if (await _uow.Tenants.ExistsBySlugAsync(normalizedSlug, ct))
            throw new ConflictException("Tenant", "slug", normalizedSlug);

        TenantEntity tenant = new()
        {
            Status = TenantStatus.Active,
            Plan = dto.Plan
        };

        TenantSettingsEntity settings = new()
        {
            Tenant = tenant,
            Name = dto.Name,
            Slug = normalizedSlug
        };

        tenant.Settings = settings;

        await _uow.Tenants.AddAsync(tenant, ct);
        await _uow.SystemOwnerAuditLogs.AddAsync(
            BuildAuditLog(systemOwnerId, SystemOwnerAuditAction.TenantCreated,
                description: $"Created tenant '{normalizedSlug}'"), ct);
        await _uow.SaveChangesAsync(ct);

        return tenant.Id;
    }

    public async Task UpdateSettingsAsync(
        Guid tenantId,
        UpdateTenantSettingsDTO dto,
        Guid systemOwnerId,
        CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantWithSettingsOrThrowAsync(tenantId, ct);

        if (tenant.Settings is null)
            throw new NotFoundException("Tenant settings not found.");

        tenant.Settings.Name = dto.Name;
        tenant.Settings.LogoUrl = dto.LogoUrl;
        tenant.Settings.FaviconUrl = dto.FaviconUrl;
        tenant.Settings.Description = dto.Description;
        tenant.Settings.PrimaryColor = dto.PrimaryColor;
        tenant.Settings.SecondaryColor = dto.SecondaryColor;
        tenant.Settings.SmtpHost = dto.SmtpHost;
        tenant.Settings.SmtpPort = dto.SmtpPort;
        tenant.Settings.SmtpSenderEmail = dto.SmtpSenderEmail;

        _uow.Tenants.Update(tenant);
        await _uow.SystemOwnerAuditLogs.AddAsync(
            BuildAuditLog(systemOwnerId, SystemOwnerAuditAction.TenantUpdated,
                description: $"Updated settings for tenant '{tenant.Settings.Slug}'"), ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task UpdatePlanAsync(
        Guid tenantId,
        UpdateTenantPlanDTO dto,
        Guid systemOwnerId,
        CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantOrThrowAsync(tenantId, ct);

        tenant.Plan = dto.Plan;

        _uow.Tenants.Update(tenant);
        await _uow.SystemOwnerAuditLogs.AddAsync(
            BuildAuditLog(systemOwnerId, SystemOwnerAuditAction.TenantPlanChanged,
                description: $"Changed plan to {dto.Plan}"), ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SuspendAsync(Guid tenantId, Guid systemOwnerId, CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantOrThrowAsync(tenantId, ct);

        if (tenant.Status is TenantStatus.Suspended)
            throw new ConflictException("Tenant is already suspended.");

        if (tenant.Status is TenantStatus.Cancelled)
            throw new ConflictException("Tenant is cancelled and cannot be suspended.");

        tenant.Status = TenantStatus.Suspended;

        _uow.Tenants.Update(tenant);
        await _uow.SystemOwnerAuditLogs.AddAsync(
            BuildAuditLog(systemOwnerId, SystemOwnerAuditAction.TenantSuspended), ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(Guid tenantId, Guid systemOwnerId, CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantOrThrowAsync(tenantId, ct);

        if (tenant.Status is TenantStatus.Cancelled)
            throw new ConflictException("Tenant is already cancelled.");

        tenant.Status = TenantStatus.Cancelled;

        _uow.Tenants.Update(tenant);
        await _uow.SystemOwnerAuditLogs.AddAsync(
            BuildAuditLog(systemOwnerId, SystemOwnerAuditAction.TenantCancelled), ct);
        await _uow.SaveChangesAsync(ct);
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private async Task<TenantEntity> GetTenantOrThrowAsync(Guid tenantId, CancellationToken ct)
    {
        TenantEntity? tenant = await _uow.Tenants.GetByIdAsync(tenantId, ct);

        if (tenant is null)
            throw new NotFoundException("Tenant", tenantId);

        return tenant;
    }

    private async Task<TenantEntity> GetTenantWithSettingsOrThrowAsync(Guid tenantId, CancellationToken ct)
    {
        TenantEntity? tenant = await _uow.Tenants.GetByIdWithSettingsAsync(tenantId, ct);

        if (tenant is null)
            throw new NotFoundException("Tenant", tenantId);

        return tenant;
    }

    private static ReadTenantDTO MapToReadTenantDTO(TenantEntity tenant) => new(
        tenant.Id,
        tenant.Settings?.Name ?? string.Empty,
        tenant.Settings?.Slug ?? string.Empty,
        tenant.Plan,
        tenant.Status,
        tenant.CreatedAt
    );

    private static ReadTenantDetailsDTO MapToDetailsDTO(TenantEntity tenant, int userCount) => new(
        tenant.Id,
        tenant.Plan,
        tenant.Status,
        tenant.CreatedAt,
        tenant.Settings?.Name ?? string.Empty,
        tenant.Settings?.Slug ?? string.Empty,
        tenant.Settings?.LogoUrl,
        tenant.Settings?.FaviconUrl,
        tenant.Settings?.Description,
        tenant.Settings?.PrimaryColor,
        tenant.Settings?.SecondaryColor,
        tenant.Settings?.SmtpHost,
        tenant.Settings?.SmtpPort,
        tenant.Settings?.SmtpSenderEmail,
        userCount
    );

    private static SystemOwnerAuditLogEntity BuildAuditLog(
        Guid systemOwnerId,
        SystemOwnerAuditAction action,
        string? description = null,
        bool isSuccess = true) => new()
    {
        SystemOwnerId = systemOwnerId,
        Action = action,
        Description = description,
        IsSuccess = isSuccess
    };
}
