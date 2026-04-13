using Domain.Entities.Common;
using Domain.Enums.System;

namespace Domain.Entities.System;

public sealed class SystemOwnerAuditLogEntity : ImmutableEntity
{
    public Guid SystemOwnerId { get; set; }
    public SystemOwnerEntity SystemOwner { get; set; } = null!;
    public SystemOwnerAuditAction Action { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
}
