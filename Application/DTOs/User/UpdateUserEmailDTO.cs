namespace Application.DTOs.User;

public sealed record UpdateUserEmailDTO
{
    public string NewEmail { get; init; } = null!;
    public string CurrentPassword { get; init; } = null!;
}
