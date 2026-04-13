using Domain.Entities.Common;

namespace Domain.Entities.System;

public sealed class SystemOwnerEntity : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string? ResetTokenHash { get; set; }
    public DateTimeOffset? ResetTokenExpiresAt { get; set; }
    public ICollection<SystemOwnerSessionEntity> Sessions { get; set; } = new List<SystemOwnerSessionEntity>();
    public ICollection<SystemOwnerAuditLogEntity> AuditLogs { get; set; } = new List<SystemOwnerAuditLogEntity>();
}
