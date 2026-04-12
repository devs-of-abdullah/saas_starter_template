using Application.DTOs.User;
using FluentValidation;

namespace Application.Validators.User;

public sealed class UpdateUserEmailDTOValidator : AbstractValidator<UpdateUserEmailDTO>
{
    public UpdateUserEmailDTOValidator()
    {
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(100);
    }
}
