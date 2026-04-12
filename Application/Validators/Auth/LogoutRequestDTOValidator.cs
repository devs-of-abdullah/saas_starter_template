using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class LogoutRequestDTOValidator : AbstractValidator<LogoutRequestDTO>
{
    public LogoutRequestDTOValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
