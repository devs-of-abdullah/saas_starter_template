using Domain.Entities.Common;

namespace Domain.Entities.System;

public sealed class SystemOwnerSessionEntity : BaseEntity
{
    public Guid SystemOwnerId { get; set; }
    public SystemOwnerEntity SystemOwner { get; set; } = null!;
    public string RefreshTokenHash { get; set; } = null!;
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    public DateTimeOffset? RefreshTokenRevokedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTimeOffset LastUsedAt { get; set; }
}
