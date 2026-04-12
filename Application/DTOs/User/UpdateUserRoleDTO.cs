using Domain.Enums.User;

namespace Application.DTOs.User;

public sealed record UpdateUserRoleDTO
{
    public UserRole NewRole { get; init; }
}
