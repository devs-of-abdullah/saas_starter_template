using Application.DTOs.User;
using FluentValidation;

namespace Application.Validators.User;

public sealed class UpdateUserPasswordDTOValidator : AbstractValidator<UpdateUserPasswordDTO>
{
    public UpdateUserPasswordDTOValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(100);

        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100).Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$").WithMessage("Password must contain uppercase, lowercase, digit, and special character.");

        RuleFor(x => x.ConfirmNewPassword).NotEmpty().MaximumLength(100).Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
