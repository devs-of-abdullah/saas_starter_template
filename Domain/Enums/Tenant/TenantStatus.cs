namespace Domain.Enums.Tenant;

public enum TenantStatus
{
    Active = 0,
    Suspended = 1,
    Inactive = 2,
}
public static class TenantStatuses
{
    public const string Active = nameof(TenantStatus.Active);
    public const string Suspended = nameof(TenantStatus.Suspended);
    public const string Inactive = nameof(TenantStatus.Inactive);
}
