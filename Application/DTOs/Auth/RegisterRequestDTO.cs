namespace Application.DTOs.Auth;

public sealed record RegisterRequestDTO(
    string Email,
    string Password,
    string TenantSlug
);
