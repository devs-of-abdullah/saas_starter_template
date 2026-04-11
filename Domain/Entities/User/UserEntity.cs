using Domain.Entities.Common;
using Domain.Enums.User;
using Domain.Entities.Tenant;

namespace Domain.Entities.User;
public class UserEntity : BaseEntity
{   public Guid TenantId { get; set; }
    public TenantEntity Tenant { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.TenantUser;
    public UserStatus Status { get; set; } = UserStatus.PendingVerification;

    public string? ResetTokenHash { get; set; }
    public DateTimeOffset? ResetTokenExpiresAt { get; set; }

    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTimeOffset? EmailVerificationTokenExpiresAt { get; set; }
    public DateTimeOffset? EmailVerificationTokenSentAt { get; set; }
    public string? PendingEmail { get; set; }
    public string? PendingEmailTokenHash { get; set; }
    public DateTimeOffset? PendingEmailTokenExpiresAt { get; set; }
    public ICollection<UserSessionEntity> Sessions { get; set; } = new List<UserSessionEntity>();
    public ICollection<AuditLogEntity> AuditLogs { get; set; } = new List<AuditLogEntity>();
}



