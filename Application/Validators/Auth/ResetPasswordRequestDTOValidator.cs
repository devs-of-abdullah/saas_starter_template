using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class ResetPasswordRequestDTOValidator : AbstractValidator<ResetPasswordRequestDTO>
{
    public ResetPasswordRequestDTOValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches(@"^\d{6}$").WithMessage("Code must be exactly 6 digits.");

        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100).Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$").WithMessage("Password must contain uppercase, lowercase, digit, and special character.");
    }
}
