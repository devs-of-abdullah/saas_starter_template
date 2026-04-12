namespace Application.DTOs.Auth;

public sealed record ResendVerificationRequestDTO(
    string Email,
    string TenantSlug
);
