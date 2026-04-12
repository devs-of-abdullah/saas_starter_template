namespace Application.DTOs.Auth;

public sealed record ForgotPasswordRequestDTO(string Email,string TenantSlug);
