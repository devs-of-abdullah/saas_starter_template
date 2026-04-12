namespace Application.DTOs.Auth;

public sealed record LogoutRequestDTO(
    string RefreshToken
);
