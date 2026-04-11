using Domain.Enums.User;
using System.ComponentModel.DataAnnotations;

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

public sealed record UpdateUserPasswordDTO
{
    [Required, MinLength(8), MaxLength(100)]
    public string CurrentPassword { get; init; } = null!;

    [Required, MinLength(8), MaxLength(100)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
    public string NewPassword { get; init; } = null!;

    [Required, MaxLength(100)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmNewPassword { get; init; } = null!;
}

public sealed record UpdateUserEmailDTO
{
    [Required, EmailAddress, MaxLength(256)]
    public string NewEmail { get; init; } = null!;

    [Required, MinLength(8), MaxLength(100)]
    public string CurrentPassword { get; init; } = null!;
}

public sealed record UpdateUserRoleDTO
{
    [Required]
    public UserRole NewRole { get; init; }
}

public sealed record DeleteUserDTO
{
    [Required, MinLength(8), MaxLength(100)]
    public string CurrentPassword { get; init; } = null!;
}