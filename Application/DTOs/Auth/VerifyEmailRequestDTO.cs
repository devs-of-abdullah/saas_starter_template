namespace Application.DTOs.Auth;

public sealed record VerifyEmailRequestDTO(
    string Code
);
