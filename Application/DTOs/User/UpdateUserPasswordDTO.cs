namespace Application.DTOs.User;

public sealed record UpdateUserPasswordDTO
{
    public string CurrentPassword { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
    public string ConfirmNewPassword { get; init; } = null!;
}
