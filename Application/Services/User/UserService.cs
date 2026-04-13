using Application.Common;
using Application.DTOs.User;
using Application.Interfaces.Common;
using Application.Interfaces.Email;
using Application.Interfaces.Services.User;
using Domain.Entities.User;
using Domain.Enums.User;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using ValidationException = Domain.Exceptions.ValidationException;

namespace Application.Services.User;

public sealed class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<UserEntity> _hasher;
    private readonly IEmailBackgroundQueue _emailQueue;

    public UserService(IUnitOfWork uow, IPasswordHasher<UserEntity> hasher, IEmailBackgroundQueue emailQueue)
    {
        _uow = uow;
        _hasher = hasher;
        _emailQueue = emailQueue;
    }

    public async Task<ReadUserDTO> GetByIdAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        UserEntity user = await GetUserInTenantAsync(userId, tenantId, ct);
        return MapToDTO(user);
    }

    public async Task<IReadOnlyList<ReadUserDTO>> GetAllByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1)
            throw new ValidationException(nameof(page), "Page must be 1 or greater.");

        if (pageSize < 1 || pageSize > 100)
            throw new ValidationException(nameof(pageSize), "Page size must be between 1 and 100.");

        IReadOnlyList<UserEntity> users = await _uow.Users.GetByTenantIdPagedAsync(tenantId, page, pageSize, ct);
        return users.Select(MapToDTO).ToList();
    }

    public async Task UpdatePasswordAsync(Guid userId, Guid tenantId, UpdateUserPasswordDTO dto, CancellationToken ct = default)
    {
        UserEntity user = await GetUserInTenantAsync(userId, tenantId, ct);

        if (dto.CurrentPassword == dto.NewPassword)
            throw new ValidationException(nameof(dto.NewPassword), "New password must differ from current password.");

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Current password is incorrect.");

        user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);

        await _uow.UserSessions.RevokeAllByUserIdAsync(user.Id, DateTimeOffset.UtcNow, ct);
        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, user.TenantId, AuditAction.PasswordChanged), ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task UpdateEmailAsync(Guid userId, Guid tenantId, UpdateUserEmailDTO dto, CancellationToken ct = default)
    {
        UserEntity user = await GetUserInTenantAsync(userId, tenantId, ct);

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Current password is incorrect.");

        string newEmail = dto.NewEmail.Trim().ToLowerInvariant();

        if (newEmail == user.Email)
            throw new ValidationException(nameof(dto.NewEmail), "New email must be different from current email.");

        if (await _uow.Users.ExistsByEmailAsync(newEmail, tenantId, ct))
            throw new ConflictException("Email is already in use.");

        string code = CryptoHelpers.GenerateSecureCode();
        user.PendingEmail = newEmail;
        user.PendingEmailTokenHash = CryptoHelpers.HashToken(code);
        user.PendingEmailTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1);

        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, user.TenantId, AuditAction.EmailChangeRequested), ct);
        await _uow.SaveChangesAsync(ct);

        _emailQueue.EnqueueEmailChange(newEmail, code);
    }

    public async Task<bool> ConfirmEmailChangeAsync(string code, CancellationToken ct = default)
    {
        string tokenHash = CryptoHelpers.HashToken(code);
        UserEntity? user = await _uow.Users.GetByPendingEmailTokenHashAsync(tokenHash, ct);

        if (user is null) return false;

        if (user.PendingEmailTokenExpiresAt is null ||
            user.PendingEmailTokenExpiresAt <= DateTimeOffset.UtcNow)
            return false;

        user.Email = user.PendingEmail!;
        user.PendingEmail = null;
        user.PendingEmailTokenHash = null;
        user.PendingEmailTokenExpiresAt = null;

        await _uow.UserSessions.RevokeAllByUserIdAsync(user.Id, DateTimeOffset.UtcNow, ct);
        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, user.TenantId, AuditAction.EmailChanged), ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task UpdateRoleAsync(Guid userId, Guid tenantId, UpdateUserRoleDTO dto, CancellationToken ct = default)
    {
        UserEntity user = await GetUserInTenantAsync(userId, tenantId, ct);

        if (user.Role == dto.NewRole) return;

        UserRole previousRole = user.Role;
        user.Role = dto.NewRole;

        await _uow.AuditLogs.AddAsync(
            BuildLog(user.Id, user.TenantId, AuditAction.RoleChanged, description: $"{previousRole} → {dto.NewRole}"), ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid userId, Guid tenantId, DeleteUserDTO dto, CancellationToken ct = default)
    {
        UserEntity user = await GetUserInTenantAsync(userId, tenantId, ct);

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Current password is incorrect.");

        await _uow.UserSessions.RevokeAllByUserIdAsync(user.Id, DateTimeOffset.UtcNow, ct);
        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, user.TenantId, AuditAction.UserDeleted), ct);

        _uow.Users.Delete(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task AdminDeleteAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        UserEntity user = await GetUserInTenantAsync(userId, tenantId, ct);

        await _uow.UserSessions.RevokeAllByUserIdAsync(user.Id, DateTimeOffset.UtcNow, ct);
        await _uow.AuditLogs.AddAsync(BuildLog(user.Id, user.TenantId, AuditAction.UserDeleted), ct);

        _uow.Users.Delete(user);
        await _uow.SaveChangesAsync(ct);
    }



    async Task<UserEntity> GetUserInTenantAsync(Guid userId, Guid tenantId, CancellationToken ct)
    {
        UserEntity? user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User '{userId}' not found.");

        if (user.TenantId != tenantId)
            throw new NotFoundException($"User '{userId}' not found.");

        return user;
    }

    static ReadUserDTO MapToDTO(UserEntity user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Role = user.Role,
        IsEmailVerified = user.IsEmailVerified,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    static AuditLogEntity BuildLog(Guid userId,Guid tenantId,AuditAction action,string? description = null,bool isSuccess = true) => new()
    {
        UserId = userId,
        TenantId = tenantId,
        Action = action,
        Description = description,
        IsSuccess = isSuccess
    };
}
