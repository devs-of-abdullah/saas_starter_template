using Application.DTOs.System;

namespace Application.Interfaces.System;

public interface ISystemOwnerAuthService
{
    Task<SystemOwnerTokenResponseDTO> LoginAsync(
        SystemOwnerLoginRequestDTO request,
        string? ipAddress,
        string? userAgent,
        string? deviceInfo,
        CancellationToken ct = default);

    Task<SystemOwnerTokenResponseDTO> RefreshAsync(SystemOwnerRefreshRequestDTO request, CancellationToken ct = default);

    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

    Task ForgotPasswordAsync(SystemOwnerForgotPasswordRequestDTO request, CancellationToken ct = default);

    Task<bool> ResetPasswordAsync(SystemOwnerResetPasswordRequestDTO request, CancellationToken ct = default);

    Task ChangePasswordAsync(Guid systemOwnerId, SystemOwnerChangePasswordRequestDTO request, CancellationToken ct = default);
}
