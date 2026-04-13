namespace Application.DTOs.Tenant;

public sealed record ReadTenantDTO(
    Guid Id,
    string Name,
    string Slug,
    Domain.Enums.Tenant.TenantPlan Plan,
    Domain.Enums.Tenant.TenantStatus Status,
    DateTimeOffset CreatedAt
);
