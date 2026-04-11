namespace Application.DTOs.Auth;

public enum EmailVerificationResult
{
    Success,
    AlreadyVerified,
    Expired,
    InvalidCode
}
