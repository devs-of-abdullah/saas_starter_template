using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Application.Interfaces.Email;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Tenant;
using Application.Interfaces.Services.User;
using Application.Services.Auth;
using Application.Services.User;
using Domain.Entities.User;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories.Tenant;
using Infrastructure.Persistence.Repositories.User;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Email;
using Infrastructure.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure;

/// <summary>Registers all Infrastructure-layer services with the DI container.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds settings, repositories, unit of work, domain services, and email services
    /// to the service collection.
    /// </summary>
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // -------------------------------------------------------------------------
        // Settings
        // -------------------------------------------------------------------------

        services.Configure<Application.Settings.Auth.JwtSettings>(opts =>
            configuration.GetSection("JwtSettings").Bind(opts));

        services.Configure<Application.Settings.Email.EmailSettings>(opts =>
            configuration.GetSection("EmailSettings").Bind(opts));

        // -------------------------------------------------------------------------
        // Persistence
        // -------------------------------------------------------------------------

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // -------------------------------------------------------------------------
        // Repositories — scoped so they share the same DbContext per request
        // -------------------------------------------------------------------------

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ITenantSettingsRepository, TenantSettingsRepository>();

        // -------------------------------------------------------------------------
        // Identity
        // -------------------------------------------------------------------------

        services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();

        // -------------------------------------------------------------------------
        // Services
        // -------------------------------------------------------------------------

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<ITokenService, TokenService>();

        // -------------------------------------------------------------------------
        // Email
        // -------------------------------------------------------------------------

        // Register as a singleton so the same Channel instance is shared between
        // IEmailBackgroundQueue (called by services) and IHostedService (the dequeue loop).
        services.AddSingleton<EmailBackgroundQueue>();
        services.AddSingleton<IEmailBackgroundQueue>(sp => sp.GetRequiredService<EmailBackgroundQueue>());
        services.AddHostedService(sp => sp.GetRequiredService<EmailBackgroundQueue>());
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}
