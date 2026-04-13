using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Tenant;

public sealed record UpdateTenantPlanDTO(
    [Required]
    Domain.Enums.Tenant.TenantPlan Plan
);
