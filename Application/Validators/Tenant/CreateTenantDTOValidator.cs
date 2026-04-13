using Application.DTOs.Tenant;
using FluentValidation;

namespace Application.Validators.Tenant;

/// <summary>Validates CreateTenantDTO.</summary>
public sealed class CreateTenantDTOValidator : AbstractValidator<CreateTenantDTO>
{
    public CreateTenantDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MinimumLength(3).WithMessage("Slug must be at least 3 characters.")
            .MaximumLength(63).WithMessage("Slug cannot exceed 63 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.");
    }
}
