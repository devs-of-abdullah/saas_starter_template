namespace Domain.Enums.System;

public enum SystemOwnerAuditAction
{
    Login              = 0,
    LoginFailed        = 1,
    Logout             = 2,
    TokenTheftDetected = 3,
    PasswordReset      = 4,
    PasswordChanged    = 5,
    TenantCreated      = 6,
    TenantUpdated      = 7,
    TenantSuspended    = 8,
    TenantCancelled    = 9,
    TenantPlanChanged  = 10,
}
