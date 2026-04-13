using Application.Common;
using Application.DTOs.System;
using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Application.Interfaces.Email;
using Application.Interfaces.System;
using Application.Settings.Auth;
using Domain.Entities.System;
using Domain.Enums.System;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Services.System;

public sealed class SystemOwnerAuthService : ISystemOwnerAuthService
{
    readonly IUnitOfWork _uow;
    readonly ITokenService _tokenService;
    readonly IEmailOutbox _emailOutbox;
    readonly IPasswordHasher<SystemOwnerEntity> _hasher;
    readonly JwtSettings _settings;

    const int MaxIpAddressLength  = 45;
    const int MaxUserAgentLength  = 512;
    const int MaxDeviceInfoLength = 512;

    // Timing attack prevention — bcrypt always runs even when owner not found
    static readonly SystemOwnerEntity DummyOwner = new() { PasswordHash = string.Empty };

    public SystemOwnerAuthService(
        IUnitOfWork uow,
        ITokenService tokenService,
        IEmailOutbox emailOutbox,
        IPasswordHasher<SystemOwnerEntity> hasher,
        IOptions<JwtSettings> settings)
    {
        _uow = uow;
        _tokenService = tokenService;
        _emailOutbox = emailOutbox;
        _hasher = hasher;
        _settings = settings.Value;
    }

    public async Task<SystemOwnerTokenResponseDTO> LoginAsync(
        SystemOwnerLoginRequestDTO request,
        string? ipAddress,
        string? userAgent,
        string? deviceInfo,
        CancellationToken ct = default)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        SystemOwnerEntity? owner = await _uow.SystemOwners.GetByEmailAsync(email, ct);

        SystemOwnerEntity targetOwner = owner ?? DummyOwner;
        string targetHash = owner is not null ? owner.PasswordHash : _hasher.HashPassword(DummyOwner, "Invalid123!");
        PasswordVerificationResult verifyResult = _hasher.VerifyHashedPassword(targetOwner, targetHash, request.Password);
        bool passwordValid = owner is not null && verifyResult != PasswordVerificationResult.Failed;

        if (!passwordValid)
        {
            if (owner is not null)
                await _uow.SystemOwnerAuditLogs.AddAsync(BuildLog(owner.Id, SystemOwnerAuditAction.LoginFailed,
                    ipAddress: Truncate(ipAddress, MaxIpAddressLength),
                    userAgent: Truncate(userAgent, MaxUserAgentLength),
                    isSuccess: false), ct);

            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!owner!.IsActive)
            throw new ForbiddenException("This account has been deactivated.");

        (string accessToken, string refreshToken, SystemOwnerSessionEntity session) =
            CreateSession(owner,
                Truncate(ipAddress, MaxIpAddressLength),
                Truncate(userAgent, MaxUserAgentLength),
                Truncate(deviceInfo, MaxDeviceInfoLength));

        await _uow.SystemOwnerSessions.AddAsync(session, ct);
        await _uow.SystemOwnerAuditLogs.AddAsync(BuildLog(owner.Id, SystemOwnerAuditAction.Login,
            ipAddress: Truncate(ipAddress, MaxIpAddressLength),
            userAgent: Truncate(userAgent, MaxUserAgentLength)), ct);

        await _uow.SaveChangesAsync(ct);
        return new SystemOwnerTokenResponseDTO(accessToken, refreshToken, session.RefreshTokenExpiresAt);
    }

    public async Task<SystemOwnerTokenResponseDTO> RefreshAsync(
        SystemOwnerRefreshRequestDTO request,
        CancellationToken ct = default)
    {
        string tokenHash = CryptoHelpers.HashToken(request.RefreshToken);
        SystemOwnerSessionEntity? session = await _uow.SystemOwnerSessions.GetByRefreshTokenHashAsync(tokenHash, ct);

        if (session is not null && session.RefreshTokenRevokedAt.HasValue)
        {
            // Token theft detected — revoke all sessions
            await _uow.SystemOwnerSessions.RevokeAllBySystemOwnerIdAsync(session.SystemOwnerId, DateTimeOffset.UtcNow, ct);
            await _uow.SystemOwnerAuditLogs.AddAsync(
                BuildLog(session.SystemOwnerId, SystemOwnerAuditAction.TokenTheftDetected, isSuccess: false), ct);
            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedException("Security alert: your session was invalidated. Please log in again.");
        }

        if (session is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        if (session.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow)
            throw new UnauthorizedException("Refresh token has expired. Please log in again.");

        SystemOwnerEntity owner = await _uow.SystemOwners.GetByIdAsync(session.SystemOwnerId, ct)
            ?? throw new UnauthorizedException("System owner not found.");

        if (!owner.IsActive)
            throw new ForbiddenException("This account has been deactivated.");

        // Rotation — revoke old, issue new
        session.RefreshTokenRevokedAt = DateTimeOffset.UtcNow;

        (string accessToken, string refreshToken, SystemOwnerSessionEntity newSession) =
            CreateSession(owner, session.IpAddress, session.UserAgent, session.DeviceInfo);

        await _uow.SystemOwnerSessions.AddAsync(newSession, ct);
        await _uow.SaveChangesAsync(ct);
        return new SystemOwnerTokenResponseDTO(accessToken, refreshToken, newSession.RefreshTokenExpiresAt);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        string tokenHash = CryptoHelpers.HashToken(refreshToken);
        SystemOwnerSessionEntity? session = await _uow.SystemOwnerSessions.GetByRefreshTokenHashAsync(tokenHash, ct);

        if (session is null || session.RefreshTokenRevokedAt.HasValue) return;

        session.RefreshTokenRevokedAt = DateTimeOffset.UtcNow;
        await _uow.SystemOwnerAuditLogs.AddAsync(
            BuildLog(session.SystemOwnerId, SystemOwnerAuditAction.Logout), ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(
        SystemOwnerForgotPasswordRequestDTO request,
        CancellationToken ct = default)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        SystemOwnerEntity? owner = await _uow.SystemOwners.GetByEmailAsync(email, ct);

        // Safe enumeration — always return without revealing existence
        if (owner is null) return;

        // Cooldown: 1 minute between requests
        if (owner.ResetTokenExpiresAt.HasValue &&
            owner.ResetTokenExpiresAt.Value.AddHours(1).Subtract(TimeSpan.FromMinutes(59)) > DateTimeOffset.UtcNow)
            return;

        string code = CryptoHelpers.GenerateSecureCode();
        owner.ResetTokenHash = CryptoHelpers.HashToken(code);
        owner.ResetTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1);

        _emailOutbox.AddPasswordReset(owner.Email, code);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> ResetPasswordAsync(
        SystemOwnerResetPasswordRequestDTO request,
        CancellationToken ct = default)
    {
        string tokenHash = CryptoHelpers.HashToken(request.Code);
        SystemOwnerEntity? owner = await _uow.SystemOwners.GetByResetTokenHashAsync(tokenHash, ct);

        if (owner is null) return false;

        if (owner.ResetTokenExpiresAt is null || owner.ResetTokenExpiresAt <= DateTimeOffset.UtcNow)
            return false;

        owner.PasswordHash = _hasher.HashPassword(owner, request.NewPassword);
        owner.ResetTokenHash = null;
        owner.ResetTokenExpiresAt = null;

        await _uow.SystemOwnerSessions.RevokeAllBySystemOwnerIdAsync(owner.Id, DateTimeOffset.UtcNow, ct);
        await _uow.SystemOwnerAuditLogs.AddAsync(
            BuildLog(owner.Id, SystemOwnerAuditAction.PasswordReset), ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task ChangePasswordAsync(
        Guid systemOwnerId,
        SystemOwnerChangePasswordRequestDTO request,
        CancellationToken ct = default)
    {
        SystemOwnerEntity owner = await _uow.SystemOwners.GetByIdAsync(systemOwnerId, ct)
            ?? throw new NotFoundException("System owner", systemOwnerId);

        if (request.CurrentPassword == request.NewPassword)
            throw new ValidationException(nameof(request.NewPassword), "New password must differ from current password.");

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(owner, owner.PasswordHash, request.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Current password is incorrect.");

        owner.PasswordHash = _hasher.HashPassword(owner, request.NewPassword);

        await _uow.SystemOwnerSessions.RevokeAllBySystemOwnerIdAsync(owner.Id, DateTimeOffset.UtcNow, ct);
        await _uow.SystemOwnerAuditLogs.AddAsync(
            BuildLog(owner.Id, SystemOwnerAuditAction.PasswordChanged), ct);
        await _uow.SaveChangesAsync(ct);
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    (string accessToken, string refreshToken, SystemOwnerSessionEntity session) CreateSession(
        SystemOwnerEntity owner,
        string? ipAddress,
        string? userAgent,
        string? deviceInfo)
    {
        string accessToken = _tokenService.GenerateSystemOwnerAccessToken(owner);
        string refreshToken = CryptoHelpers.GenerateSecureToken();

        SystemOwnerSessionEntity session = new()
        {
            SystemOwnerId = owner.Id,
            RefreshTokenHash = CryptoHelpers.HashToken(refreshToken),
            RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDays),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceInfo = deviceInfo,
            LastUsedAt = DateTimeOffset.UtcNow
        };

        return (accessToken, refreshToken, session);
    }

    static SystemOwnerAuditLogEntity BuildLog(
        Guid systemOwnerId,
        SystemOwnerAuditAction action,
        string? ipAddress = null,
        string? userAgent = null,
        string? description = null,
        bool isSuccess = true) => new()
    {
        SystemOwnerId = systemOwnerId,
        Action = action,
        IpAddress = ipAddress,
        UserAgent = userAgent,
        Description = description,
        IsSuccess = isSuccess
    };

    static string? Truncate(string? value, int maxLength) =>
        value is null ? null : value[..Math.Min(value.Length, maxLength)];
}
