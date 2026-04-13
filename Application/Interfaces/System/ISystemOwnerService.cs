using Application.DTOs.System;

namespace Application.Interfaces.System;

public interface ISystemOwnerService
{
    Task<SystemOwnerProfileDTO> GetProfileAsync(Guid systemOwnerId, CancellationToken ct = default);
    Task<IReadOnlyList<SystemOwnerSessionDTO>> GetSessionsAsync(Guid systemOwnerId, CancellationToken ct = default);
    Task RevokeSessionAsync(Guid systemOwnerId, Guid sessionId, CancellationToken ct = default);
}
