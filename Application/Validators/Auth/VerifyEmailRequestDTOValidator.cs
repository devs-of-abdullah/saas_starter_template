using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class VerifyEmailRequestDTOValidator : AbstractValidator<VerifyEmailRequestDTO>
{
    public VerifyEmailRequestDTOValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches(@"^\d{6}$").WithMessage("Code must be exactly 6 digits.");
    }
}
