namespace Domain.Enums.User;

public enum AuditAction
{
    Login = 0,
    Logout = 1,
    PasswordReset = 2,
    EmailVerified = 3,
}
public static class AuditActions
{
    public const string Login = nameof(AuditAction.Login);
    public const string Logout = nameof(AuditAction.Logout);
    public const string PasswordReset = nameof(AuditAction.PasswordReset);
    public const string EmailVerified = nameof(AuditAction.EmailVerified);
}