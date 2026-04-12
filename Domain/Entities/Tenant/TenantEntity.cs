using Domain.Entities.Common;
using Domain.Entities.User;
using Domain.Enums.Tenant;

namespace Domain.Entities.Tenant;

public sealed class TenantEntity : BaseEntity
{
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public TenantPlan Plan { get; set; } = TenantPlan.Free;
    public DateTimeOffset? SubscriptionEndsAt { get; set; }
    public TenantSettingsEntity? Settings { get; set; }
    public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
}
