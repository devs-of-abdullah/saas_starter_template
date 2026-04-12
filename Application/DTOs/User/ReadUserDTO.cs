using Domain.Enums.User;

namespace Application.DTOs.User;

public sealed record ReadUserDTO
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public UserRole Role { get; init; }
    public bool IsEmailVerified { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
