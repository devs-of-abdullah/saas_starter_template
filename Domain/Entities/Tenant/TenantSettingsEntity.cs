using Domain.Entities.Common;

namespace Domain.Entities.Tenant;

public sealed class TenantSettingsEntity : BaseEntity
{
    public Guid TenantId { get; set; }
    public TenantEntity Tenant { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? Description { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }

    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpSenderEmail { get; set; }
    public string? SmtpSenderPasswordEncrypted { get; set; }
}
