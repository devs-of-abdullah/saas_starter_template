using Application.DTOs.Tenant;
using Application.Interfaces.Common;
using Application.Interfaces.Services.Tenant;
using Domain.Entities.Tenant;
using Domain.Entities.User;
using Domain.Enums.Tenant;
using Domain.Enums.User;
using Domain.Exceptions;
using ValidationException = Domain.Exceptions.ValidationException;

namespace Application.Services.Tenant;

/// <summary>Manages tenant lifecycle, settings, and subscription.</summary>
public sealed class TenantService : ITenantService
{
    private readonly IUnitOfWork _uow;

    public TenantService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <inheritdoc/>
    public async Task<ReadTenantDTO> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantOrThrowAsync(tenantId, ct);
        return MapToDTO(tenant);
    }

    /// <inheritdoc/>
    public async Task<ReadTenantDTO> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        TenantEntity? tenant = await _uow.Tenants.GetBySlugAsync(normalizedSlug, ct);

        if (tenant is null)
            throw new NotFoundException($"Tenant with slug '{normalizedSlug}' not found.");

        return MapToDTO(tenant);
    }

    /// <inheritdoc/>
    public async Task<ReadTenantSettingsDTO> GetSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantWithSettingsOrThrowAsync(tenantId, ct);

        if (tenant.Settings is null)
            throw new NotFoundException("Tenant settings not found.");

        return new ReadTenantSettingsDTO(
            tenant.Settings.Id,
            tenant.Settings.Name,
            tenant.Settings.Slug,
            tenant.Settings.LogoUrl,
            tenant.Settings.FaviconUrl,
            tenant.Settings.Description,
            tenant.Settings.PrimaryColor,
            tenant.Settings.SecondaryColor,
            tenant.Settings.SmtpHost,
            tenant.Settings.SmtpPort,
            tenant.Settings.SmtpSenderEmail
        );
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReadTenantDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1)
            throw new ValidationException(nameof(page), "Page must be 1 or greater.");

        if (pageSize < 1 || pageSize > 100)
            throw new ValidationException(nameof(pageSize), "Page size must be between 1 and 100.");

        IReadOnlyList<TenantEntity> tenants = await _uow.Tenants.GetAllPagedAsync(page, pageSize, ct);
        return tenants.Select(MapToDTO).ToList();
    }

    /// <inheritdoc/>
    public async Task<Guid> CreateAsync(CreateTenantDTO dto, CancellationToken ct = default)
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
        await _uow.AuditLogs.AddAsync(BuildLog(tenant.Id, AuditAction.TenantCreated), ct);
        await _uow.SaveChangesAsync(ct);

        return tenant.Id;
    }

    /// <inheritdoc/>
    public async Task UpdateSettingsAsync(Guid tenantId, UpdateTenantSettingsDTO dto, CancellationToken ct = default)
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
        await _uow.AuditLogs.AddAsync(BuildLog(tenant.Id, AuditAction.TenantUpdated), ct);
        await _uow.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task UpdatePlanAsync(Guid tenantId, UpdateTenantPlanDTO dto, CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantOrThrowAsync(tenantId, ct);

        tenant.Plan = dto.Plan;

        _uow.Tenants.Update(tenant);
        await _uow.AuditLogs.AddAsync(BuildLog(tenant.Id, AuditAction.TenantPlanChanged), ct);
        await _uow.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task SuspendAsync(Guid tenantId, CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantOrThrowAsync(tenantId, ct);

        if (tenant.Status is TenantStatus.Suspended)
            throw new ConflictException("Tenant is already suspended.");

        if (tenant.Status is TenantStatus.Cancelled)
            throw new ConflictException("Tenant is cancelled and cannot be suspended.");

        tenant.Status = TenantStatus.Suspended;

        _uow.Tenants.Update(tenant);
        await _uow.AuditLogs.AddAsync(BuildLog(tenant.Id, AuditAction.TenantSuspended), ct);
        await _uow.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task CancelAsync(Guid tenantId, CancellationToken ct = default)
    {
        TenantEntity tenant = await GetTenantOrThrowAsync(tenantId, ct);

        if (tenant.Status is TenantStatus.Cancelled)
            throw new ConflictException("Tenant is already cancelled.");

        tenant.Status = TenantStatus.Cancelled;

        _uow.Tenants.Update(tenant);
        await _uow.AuditLogs.AddAsync(BuildLog(tenant.Id, AuditAction.TenantCancelled), ct);
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

    private ReadTenantDTO MapToDTO(TenantEntity tenant) => new(
        tenant.Id,
        tenant.Settings?.Name ?? string.Empty,
        tenant.Settings?.Slug ?? string.Empty,
        tenant.Plan,
        tenant.Status,
        tenant.CreatedAt
    );

    private static AuditLogEntity BuildLog(Guid tenantId, AuditAction action, bool isSuccess = true) => new()
    {
        UserId = tenantId,  // For tenant operations, we record the tenant ID as UserId for simplicity
        TenantId = tenantId,
        Action = action,
        IsSuccess = isSuccess
    };
}
