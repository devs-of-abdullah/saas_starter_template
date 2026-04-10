using Domain.Entities.Common;
using Domain.Entities.Tenant;
using Domain.Enums.User;

namespace Domain.Entities.User;

public class AuditLogEntity : ImmutableEntity
{

    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid TenantId { get; set; }
    public TenantEntity Tenant { get; set; } = null!;
    public AuditAction Action { get; set; }   
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
}