using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class ResendVerificationRequestDTOValidator : AbstractValidator<ResendVerificationRequestDTO>
{
    public ResendVerificationRequestDTOValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(100);
    }
}
