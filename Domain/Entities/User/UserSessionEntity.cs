using Domain.Entities.Common;
using Domain.Entities.Tenant;

namespace Domain.Entities.User;

public sealed class UserSessionEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid TenantId { get; set; }
    public TenantEntity Tenant { get; set; } = null!;

    public string RefreshTokenHash { get; set; } = null!;
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    public DateTimeOffset? RefreshTokenRevokedAt { get; set; }

    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
}
