using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class LoginRequestDTOValidator : AbstractValidator<LoginRequestDTO>
{
    public LoginRequestDTOValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(100);
    }
}
