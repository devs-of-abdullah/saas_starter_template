using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class RegisterRequestDTOValidator : AbstractValidator<RegisterRequestDTO>
{
    public RegisterRequestDTOValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100).Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$").WithMessage("Password must contain uppercase, lowercase, digit, and special character.");

        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(100);
    }
}
