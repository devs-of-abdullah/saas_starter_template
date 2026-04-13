using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.System;

public sealed record SystemOwnerLoginRequestDTO(
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    string Email,

    [Required]
    [MaxLength(100)]
    string Password
);

public sealed record SystemOwnerTokenResponseDTO(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);

public sealed record SystemOwnerRefreshRequestDTO(
    [Required]
    string RefreshToken
);

public sealed record SystemOwnerLogoutRequestDTO(
    [Required]
    string RefreshToken
);

public sealed record SystemOwnerForgotPasswordRequestDTO(
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    string Email
);

public sealed record SystemOwnerResetPasswordRequestDTO(
    [Required]
    string Code,

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must have uppercase, lowercase, digit, and special character.")]
    string NewPassword
);

public sealed record SystemOwnerChangePasswordRequestDTO
{
    [Required]
    [MaxLength(100)]
    public string CurrentPassword { get; init; } = null!;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must have uppercase, lowercase, digit, and special character.")]
    public string NewPassword { get; init; } = null!;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmNewPassword { get; init; } = null!;
}

public sealed record SystemOwnerProfileDTO(
    Guid Id,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public sealed record SystemOwnerSessionDTO(
    Guid Id,
    string? IpAddress,
    string? UserAgent,
    string? DeviceInfo,
    DateTimeOffset LastUsedAt,
    DateTimeOffset RefreshTokenExpiresAt,
    bool IsRevoked
);
