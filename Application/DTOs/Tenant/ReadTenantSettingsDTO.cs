namespace Application.DTOs.Tenant;

public sealed record ReadTenantSettingsDTO(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string? FaviconUrl,
    string? Description,
    string? PrimaryColor,
    string? SecondaryColor,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpSenderEmail
);
