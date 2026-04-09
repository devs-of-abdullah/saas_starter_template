namespace Domain.Enums.Tenant;

public enum TenantPlan
{
    Free = 0,
    Pro = 1,
    Enterprise = 2
}

public static class TenantPlans
{
    public const string Free = nameof(TenantPlan.Free);
    public const string Pro = nameof(TenantPlan.Pro);
    public const string Enterprise = nameof(TenantPlan.Enterprise);

}
