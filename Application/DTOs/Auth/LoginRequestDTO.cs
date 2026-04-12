namespace Application.DTOs.Auth;

public sealed record LoginRequestDTO(
    string Email,
    string Password,
    string TenantSlug
);
