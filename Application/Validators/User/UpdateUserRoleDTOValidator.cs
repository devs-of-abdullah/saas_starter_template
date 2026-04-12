using Application.DTOs.User;
using FluentValidation;

namespace Application.Validators.User;

public sealed class UpdateUserRoleDTOValidator : AbstractValidator<UpdateUserRoleDTO>
{
    public UpdateUserRoleDTOValidator()
    {
        RuleFor(x => x.NewRole).IsInEnum();
    }
}
