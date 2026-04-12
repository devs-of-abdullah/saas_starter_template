namespace Application.DTOs.User;

public sealed record DeleteUserDTO
{
    public string CurrentPassword { get; init; } = null!;
}
