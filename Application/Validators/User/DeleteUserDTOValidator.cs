using Application.DTOs.User;
using FluentValidation;

namespace Application.Validators.User;

public sealed class DeleteUserDTOValidator : AbstractValidator<DeleteUserDTO>
{
    public DeleteUserDTOValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(100);
    }
}
