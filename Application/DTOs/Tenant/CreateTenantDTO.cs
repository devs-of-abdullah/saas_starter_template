using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Tenant;

public sealed record CreateTenantDTO(
    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    [MinLength(3)]
    [MaxLength(63)]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens.")]
    string Slug,

    [Required]
    Domain.Enums.Tenant.TenantPlan Plan
);
