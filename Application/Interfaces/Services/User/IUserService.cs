using Application.DTOs.User;

namespace Application.Interfaces.Services.User;

public interface IUserService
{
    Task<ReadUserDTO> GetByIdAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ReadUserDTO>> GetAllByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default);
    Task UpdatePasswordAsync(Guid userId, Guid tenantId, UpdateUserPasswordDTO dto, CancellationToken ct = default);
    Task UpdateEmailAsync(Guid userId, Guid tenantId, UpdateUserEmailDTO dto, CancellationToken ct = default);
    Task<bool> ConfirmEmailChangeAsync(string code, CancellationToken ct = default);
    Task UpdateRoleAsync(Guid userId, Guid tenantId, UpdateUserRoleDTO dto, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid tenantId, DeleteUserDTO dto, CancellationToken ct = default);
    Task AdminDeleteAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
}
