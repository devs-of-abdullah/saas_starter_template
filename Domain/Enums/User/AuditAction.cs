namespace Domain.Enums.User;

public enum AuditAction
{
    Register = 0,
    Login = 1,
    LoginFailed = 2,
    Logout = 3,
    TokenTheftDetected = 4,
    EmailVerified = 5,
    PasswordResetRequested = 6,
    PasswordReset = 7,
    PasswordChanged = 8,
    EmailChangeRequested = 9,
    EmailChanged = 10,
    RoleChanged = 11,
    UserDeleted = 12,
}
