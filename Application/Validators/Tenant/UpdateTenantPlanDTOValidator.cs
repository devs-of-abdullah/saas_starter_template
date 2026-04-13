using Application.DTOs.Tenant;
using FluentValidation;

namespace Application.Validators.Tenant;

/// <summary>Validates UpdateTenantPlanDTO.</summary>
public sealed class UpdateTenantPlanDTOValidator : AbstractValidator<UpdateTenantPlanDTO>
{
    public UpdateTenantPlanDTOValidator()
    {
        RuleFor(x => x.Plan)
            .IsInEnum().WithMessage("Plan must be a valid tenant plan.");
    }
}
