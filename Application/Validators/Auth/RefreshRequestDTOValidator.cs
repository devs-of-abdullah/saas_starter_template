using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public sealed class RefreshRequestDTOValidator : AbstractValidator<RefreshRequestDTO>
{
    public RefreshRequestDTOValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
