namespace Application.DTOs.Auth;

public sealed record TokenResponseDTO(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);
