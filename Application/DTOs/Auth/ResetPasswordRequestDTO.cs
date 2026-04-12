namespace Application.DTOs.Auth;

public sealed record ResetPasswordRequestDTO(
    string Code,
    string NewPassword
);
