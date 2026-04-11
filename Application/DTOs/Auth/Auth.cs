namespace Application.DTOs.Auth;

public record RegisterRequestDTO(
    string Email,
    string Password,
    string TenantSlug
);

public record LoginRequestDTO(
    string Email,
    string Password,
    string TenantSlug
);

public record RefreshRequestDTO(
    string RefreshToken
);

public record VerifyEmailRequestDTO(
    string Code
);

public record ResendVerificationRequestDTO(
    string Email,
    string TenantSlug
);

public record ForgotPasswordRequestDTO(
    string Email,
    string TenantSlug
);

public record ResetPasswordRequestDTO(
    string Code,
    string NewPassword
);

public record TokenResponseDTO(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);