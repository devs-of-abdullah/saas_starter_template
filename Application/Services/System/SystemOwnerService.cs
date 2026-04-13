using Application.DTOs.System;
using Application.Interfaces.Common;
using Application.Interfaces.System;
using Domain.Entities.System;
using Domain.Exceptions;

namespace Application.Services.System;

public sealed class SystemOwnerService : ISystemOwnerService
{
    private readonly IUnitOfWork _uow;

    public SystemOwnerService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SystemOwnerProfileDTO> GetProfileAsync(Guid systemOwnerId, CancellationToken ct = default)
    {
        SystemOwnerEntity owner = await _uow.SystemOwners.GetByIdAsync(systemOwnerId, ct)
            ?? throw new NotFoundException("System owner", systemOwnerId);

        return new SystemOwnerProfileDTO(owner.Id, owner.Email, owner.IsActive, owner.CreatedAt);
    }

    public async Task<IReadOnlyList<SystemOwnerSessionDTO>> GetSessionsAsync(Guid systemOwnerId, CancellationToken ct = default)
    {
        IReadOnlyList<SystemOwnerSessionEntity> sessions =
            await _uow.SystemOwnerSessions.GetBySystemOwnerIdAsync(systemOwnerId, ct);

        return sessions.Select(s => new SystemOwnerSessionDTO(
            s.Id,
            s.IpAddress,
            s.UserAgent,
            s.DeviceInfo,
            s.LastUsedAt,
            s.RefreshTokenExpiresAt,
            s.RefreshTokenRevokedAt.HasValue
        )).ToList();
    }

    public async Task RevokeSessionAsync(Guid systemOwnerId, Guid sessionId, CancellationToken ct = default)
    {
        SystemOwnerSessionEntity? session = await _uow.SystemOwnerSessions.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("Session", sessionId);

        if (session.SystemOwnerId != systemOwnerId)
            throw new NotFoundException("Session", sessionId);

        session.RefreshTokenRevokedAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }
}
