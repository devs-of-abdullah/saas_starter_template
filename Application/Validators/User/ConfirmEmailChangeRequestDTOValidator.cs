using Application.DTOs.User;
using FluentValidation;

namespace Application.Validators.User;

/// <summary>Validates <see cref="ConfirmEmailChangeRequestDTO"/> before it reaches the service layer.</summary>
public sealed class ConfirmEmailChangeRequestDTOValidator : AbstractValidator<ConfirmEmailChangeRequestDTO>
{
    /// <summary>Initialises the rule set.</summary>
    public ConfirmEmailChangeRequestDTOValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches(@"^\d{6}$")
            .WithMessage("Code must be exactly 6 digits.");
    }
}
