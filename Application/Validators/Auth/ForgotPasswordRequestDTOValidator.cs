using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class ForgotPasswordRequestDTOValidator : AbstractValidator<ForgotPasswordRequestDTO>
{
    public ForgotPasswordRequestDTOValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(100);
    }
}
