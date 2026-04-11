using Application.DTOs.Auth;

namespace Application.Interfaces.Auth;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default);

    Task<TokenResponseDTO> LoginAsync(LoginRequestDTO request, string? ipAddress, string? userAgent, string? deviceInfo, CancellationToken ct = default);

    Task<TokenResponseDTO> RefreshAsync(RefreshRequestDTO request, CancellationToken ct = default);

    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

    Task<EmailVerificationResult> VerifyEmailAsync(VerifyEmailRequestDTO request, CancellationToken ct = default);

    Task ResendVerificationEmailAsync(ResendVerificationRequestDTO request, CancellationToken ct = default);

    Task ForgotPasswordAsync(ForgotPasswordRequestDTO request, CancellationToken ct = default);

    Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO request, CancellationToken ct = default);
}