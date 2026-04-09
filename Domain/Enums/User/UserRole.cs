namespace Domain.Enums.User;
public enum UserRole
{
    TenantUser = 0,
    TenantAdmin = 1,
    TenantSuperAdmin = 2,
    SystemOwner = 3
}

public static class UserRoles
{
    public const string TenantUser = nameof(UserRole.TenantUser);
    public const string TenantAdmin = nameof(UserRole.TenantAdmin);
    public const string TenantSuperAdmin = nameof(UserRole.TenantSuperAdmin);
    public const string SystemOwner = nameof(UserRole.SystemOwner);
}