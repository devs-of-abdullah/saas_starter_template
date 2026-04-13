using Domain.Enums.Tenant;

namespace Application.DTOs.Tenant;

public sealed record ReadTenantDetailsDTO(
    Guid Id,
    TenantPlan Plan,
    TenantStatus Status,
    DateTimeOffset CreatedAt,
    string Name,
    string Slug,
    string? LogoUrl,
    string? FaviconUrl,
    string? Description,
    string? PrimaryColor,
    string? SecondaryColor,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpSenderEmail,
    int UserCount
);
