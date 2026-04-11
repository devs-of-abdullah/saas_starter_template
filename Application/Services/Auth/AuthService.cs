using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces.Common;
using Application.Interfaces.Auth;
using Application.Interfaces.Email;
using Application.Settings.Auth;
using Domain.Entities.User;
using Domain.Enums.User;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Services;

public sealed class AuthService : IAuthService
{
    readonly IUnitOfWork _uow;
    readonly ITokenService _tokenService;
    readonly IEmailBackgroundQueue _emailQueue;
    readonly IPasswordHasher<UserEntity> _hasher;
    readonly JwtSettings _settings;
    const int MaxIpAddressLength = 45;
    const int MaxUserAgentLength = 512;
    const int MaxDeviceInfoLength = 512;

    static readonly UserEntity _dummyUser = new() { PasswordHash = string.Empty };

    public AuthService(IUnitOfWork uow, ITokenService tokenService, IEmailBackgroundQueue emailQueue,IPasswordHasher<UserEntity> hasher, IOptions<JwtSettings> settings)
    {
        _uow = uow;
        _tokenService = tokenService;
        _emailQueue = emailQueue;
        _hasher = hasher;
        _settings = settings.Value;
    }


    public async Task RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default)
    {
        var tenant = await _uow.Tenants.GetBySlugAsync(request.TenantSlug.Trim().ToLowerInvariant(), ct)
            ?? throw new NotFoundException($"Tenant '{request.TenantSlug}' not found.");

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _uow.Users.GetByEmailAsync(email, tenant.Id, ct);

        if (existing is not null)
        {
            if (existing.IsEmailVerified)
                throw new ConflictException("Email already registered.");

            if (existing.EmailVerificationTokenSentAt.HasValue &&
                existing.EmailVerificationTokenSentAt.Value.AddMinutes(1) > DateTimeOffset.UtcNow)
                throw new TooManyRequestsException("Please wait 1 minute before requesting a new code.");

            var resendCode = CryptoHelpers.GenerateSecureCode();
            existing.EmailVerificationTokenHash = CryptoHelpers.HashToken(resendCode);
            existing.EmailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
            existing.EmailVerificationTokenSentAt = DateTimeOffset.UtcNow;

            await _uow.SaveChangesAsync(ct);
            _emailQueue.EnqueueVerification(existing.Email, resendCode);
            return;
        }

        var user = new UserEntity
        {
            Email = email,
            TenantId = tenant.Id,
            Role = UserRole.TenantUser,
            Status = UserStatus.PendingVerification
        };

        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        var code = CryptoHelpers.GenerateSecureCode();
        user.EmailVerificationTokenHash = CryptoHelpers.HashToken(code);
        user.EmailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        user.EmailVerificationTokenSentAt = DateTimeOffset.UtcNow;

        await _uow.Users.AddAsync(user, ct);
        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, tenant.Id, AuditAction.Register), ct);
        await _uow.SaveChangesAsync(ct);

        _emailQueue.EnqueueVerification(user.Email, code);
    }

    

    public async Task<TokenResponseDTO> LoginAsync(LoginRequestDTO request, string? ipAddress, string? userAgent, string? deviceInfo, CancellationToken ct = default)
    {
        var tenant = await _uow.Tenants.GetBySlugAsync(request.TenantSlug.Trim().ToLowerInvariant(), ct)
            ?? throw new NotFoundException($"Tenant '{request.TenantSlug}' not found.");

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _uow.Users.GetByEmailAsync(email, tenant.Id, ct);

        var targetUser = user ?? _dummyUser;
        var targetHash = user is not null ? user.PasswordHash : CryptoHelpers.GenerateSecureToken();
        var verifyResult = _hasher.VerifyHashedPassword(targetUser, targetHash, request.Password);
        var passwordValid = user is not null && verifyResult != PasswordVerificationResult.Failed;

        if (!passwordValid)
        {
            if (user is not null)
                await _uow.AuditLogs.AddAsync(
                    BuildLog(user.Id, tenant.Id, AuditAction.LoginFailed,
                        Truncate(ipAddress, MaxIpAddressLength),
                        Truncate(userAgent, MaxUserAgentLength),
                        isSuccess: false), ct);

            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!user!.IsEmailVerified)
            throw new ForbiddenException("Please verify your email before logging in.");

        if (user.Status is UserStatus.Banned or UserStatus.Suspended)
            throw new ForbiddenException("Account is not active.");

        var (accessToken, refreshToken, session) = CreateSession(user, tenant.Id, Truncate(ipAddress, MaxIpAddressLength), Truncate(userAgent, MaxUserAgentLength), Truncate(deviceInfo, MaxDeviceInfoLength));

        await _uow.UserSessions.AddAsync(session, ct);
        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, tenant.Id, AuditAction.Login,Truncate(ipAddress, MaxIpAddressLength), Truncate(userAgent, MaxUserAgentLength)), ct);

        await _uow.SaveChangesAsync(ct);
        return new TokenResponseDTO(accessToken, refreshToken, session.RefreshTokenExpiresAt);
    }



    public async Task<TokenResponseDTO> RefreshAsync(RefreshRequestDTO request, CancellationToken ct = default)
    {
        var tokenHash = CryptoHelpers.HashToken(request.RefreshToken);
        var session = await _uow.UserSessions.GetByRefreshTokenHashAsync(tokenHash, ct);

        if (session is not null && session.RefreshTokenRevokedAt.HasValue)
        {
            await _uow.UserSessions.RevokeAllByUserIdAsync(session.UserId, DateTimeOffset.UtcNow, ct);
            await _uow.AuditLogs.AddAsync(BuildLog(session.UserId, session.TenantId, AuditAction.TokenTheftDetected, isSuccess: false), ct);
            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedException("Security alert: your session was invalidated. Please log in again.");
        }

        if (session is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        if (session.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow)
            throw new UnauthorizedException("Refresh token has expired. Please log in again.");

        var user = await _uow.Users.GetByIdAsync(session.UserId, ct)
            ?? throw new UnauthorizedException("User not found.");

        if (user.Status is UserStatus.Banned or UserStatus.Suspended)
            throw new ForbiddenException("Account is not active.");

        session.RefreshTokenRevokedAt = DateTimeOffset.UtcNow;

        var (accessToken, refreshToken, newSession) = CreateSession(user, user.TenantId, session.IpAddress, session.UserAgent, session.DeviceInfo);

        await _uow.UserSessions.AddAsync(newSession, ct);
        await _uow.SaveChangesAsync(ct);
        return new TokenResponseDTO(accessToken, refreshToken, newSession.RefreshTokenExpiresAt);
    }


    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = CryptoHelpers.HashToken(refreshToken);
        var session = await _uow.UserSessions.GetByRefreshTokenHashAsync(tokenHash, ct);

        if (session is null || session.RefreshTokenRevokedAt.HasValue) return;

        session.RefreshTokenRevokedAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<EmailVerificationResult> VerifyEmailAsync(VerifyEmailRequestDTO request, CancellationToken ct = default)
    {
        var tokenHash = CryptoHelpers.HashToken(request.Code);
        var user = await _uow.Users.GetByEmailVerificationTokenHashAsync(tokenHash, ct);

        if (user is null) return EmailVerificationResult.InvalidCode;
        if (user.IsEmailVerified) return EmailVerificationResult.AlreadyVerified;

        if (user.EmailVerificationTokenExpiresAt is null ||
            user.EmailVerificationTokenExpiresAt <= DateTimeOffset.UtcNow)
            return EmailVerificationResult.Expired;

        user.IsEmailVerified = true;
        user.Status = UserStatus.Active;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;

        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, user.TenantId, AuditAction.EmailVerified), ct);
        await _uow.SaveChangesAsync(ct);
        return EmailVerificationResult.Success;
    }


    public async Task ResendVerificationEmailAsync(ResendVerificationRequestDTO request, CancellationToken ct = default)
    {
        var tenant = await _uow.Tenants.GetBySlugAsync(request.TenantSlug.Trim().ToLowerInvariant(), ct);
        if (tenant is null) return;

        var user = await _uow.Users.GetByEmailAsync(
            request.Email.Trim().ToLowerInvariant(), tenant.Id, ct);

        if (user is null || user.IsEmailVerified) return;

        if (user.EmailVerificationTokenSentAt.HasValue &&
            user.EmailVerificationTokenSentAt.Value.AddMinutes(1) > DateTimeOffset.UtcNow)
            throw new TooManyRequestsException("Please wait 1 minute before requesting a new code.");

        var code = CryptoHelpers.GenerateSecureCode();
        user.EmailVerificationTokenHash = CryptoHelpers.HashToken(code);
        user.EmailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        user.EmailVerificationTokenSentAt = DateTimeOffset.UtcNow;

        await _uow.SaveChangesAsync(ct);
        _emailQueue.EnqueueVerification(user.Email, code);
    }



    public async Task ForgotPasswordAsync(ForgotPasswordRequestDTO request, CancellationToken ct = default)
    {
        var tenant = await _uow.Tenants.GetBySlugAsync(request.TenantSlug.Trim().ToLowerInvariant(), ct);
        if (tenant is null) return;

        var user = await _uow.Users.GetByEmailAsync(
            request.Email.Trim().ToLowerInvariant(), tenant.Id, ct);

        if (user is null) return;

        if (user.ResetTokenExpiresAt.HasValue &&
            user.ResetTokenExpiresAt.Value.AddHours(1).Subtract(TimeSpan.FromMinutes(59)) > DateTimeOffset.UtcNow)
            return;

        var code = CryptoHelpers.GenerateSecureCode();
        user.ResetTokenHash = CryptoHelpers.HashToken(code);
        user.ResetTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1);

        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, tenant.Id, AuditAction.PasswordResetRequested), ct);

        await _uow.SaveChangesAsync(ct);
        _emailQueue.EnqueuePasswordReset(user.Email, code);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO request, CancellationToken ct = default)
    {
        var tokenHash = CryptoHelpers.HashToken(request.Code);
        var user = await _uow.Users.GetByResetTokenHashAsync(tokenHash, ct);

        if (user is null) return false;

        if (user.ResetTokenExpiresAt is null ||
            user.ResetTokenExpiresAt <= DateTimeOffset.UtcNow)
            return false;

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        user.ResetTokenHash = null;
        user.ResetTokenExpiresAt = null;

        await _uow.UserSessions.RevokeAllByUserIdAsync(user.Id, DateTimeOffset.UtcNow, ct);
        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, user.TenantId, AuditAction.PasswordReset), ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

     (string accessToken, string refreshToken, UserSessionEntity session) CreateSession( UserEntity user, Guid tenantId, string? ipAddress, string? userAgent, string? deviceInfo)
     {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = CryptoHelpers.GenerateSecureToken();

        var session = new UserSessionEntity
        {
            UserId = user.Id,
            TenantId = tenantId,
            RefreshTokenHash = CryptoHelpers.HashToken(refreshToken),
            RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDays),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceInfo = deviceInfo,
            LastUsedAt = DateTimeOffset.UtcNow
        };

        return (accessToken, refreshToken, session);
    }

     static AuditLogEntity BuildLog(Guid userId, Guid tenantId, AuditAction action, string? ipAddress = null, string? userAgent = null, bool isSuccess = true) => new()
        {
            UserId = userId,
            TenantId = tenantId,
            Action = action,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccess = isSuccess
        };

    static string? Truncate(string? value, int maxLength) => value is null ? null : value[..Math.Min(value.Length, maxLength)];
}