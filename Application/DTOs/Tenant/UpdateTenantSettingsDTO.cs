using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Tenant;

public sealed record UpdateTenantSettingsDTO(
    [Required]
    [MaxLength(100)]
    string Name,

    [MaxLength(2048)]
    string? LogoUrl,

    [MaxLength(2048)]
    string? FaviconUrl,

    [MaxLength(1000)]
    string? Description,

    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "PrimaryColor must be a valid hex color.")]
    string? PrimaryColor,

    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "SecondaryColor must be a valid hex color.")]
    string? SecondaryColor,

    [MaxLength(256)]
    string? SmtpHost,

    int? SmtpPort,

    [MaxLength(256)]
    string? SmtpSenderEmail
);
