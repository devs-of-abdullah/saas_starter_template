namespace Domain.Enums.Email;

/// <summary>Identifies the type of transactional email being sent.</summary>
public enum EmailKind
{
    Verification  = 0,
    PasswordReset = 1,
    EmailChange   = 2,
}
