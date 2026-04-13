using Application.DTOs.Tenant;
using FluentValidation;

namespace Application.Validators.Tenant;

/// <summary>Validates UpdateTenantSettingsDTO.</summary>
public sealed class UpdateTenantSettingsDTOValidator : AbstractValidator<UpdateTenantSettingsDTO>
{
    public UpdateTenantSettingsDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(2048).WithMessage("Logo URL cannot exceed 2048 characters.");

        RuleFor(x => x.FaviconUrl)
            .MaximumLength(2048).WithMessage("Favicon URL cannot exceed 2048 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.PrimaryColor)
            .Matches("^#[0-9a-fA-F]{6}$").When(x => !string.IsNullOrEmpty(x.PrimaryColor))
            .WithMessage("Primary color must be a valid hex color (e.g., #FF0000).");

        RuleFor(x => x.SecondaryColor)
            .Matches("^#[0-9a-fA-F]{6}$").When(x => !string.IsNullOrEmpty(x.SecondaryColor))
            .WithMessage("Secondary color must be a valid hex color (e.g., #FF0000).");

        RuleFor(x => x.SmtpHost)
            .MaximumLength(256).WithMessage("SMTP host cannot exceed 256 characters.");

        RuleFor(x => x.SmtpPort)
            .GreaterThan(0).When(x => x.SmtpPort.HasValue)
            .WithMessage("SMTP port must be greater than 0.")
            .LessThanOrEqualTo(65535).When(x => x.SmtpPort.HasValue)
            .WithMessage("SMTP port must be 65535 or less.");

        RuleFor(x => x.SmtpSenderEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.SmtpSenderEmail))
            .WithMessage("SMTP sender email must be a valid email address.")
            .MaximumLength(256).WithMessage("SMTP sender email cannot exceed 256 characters.");
    }
}
