using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Application.Interfaces.Email;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.System;
using Application.Interfaces.Repositories.Tenant;
using Application.Interfaces.Services.Tenant;
using Application.Interfaces.Services.User;
using Application.Interfaces.System;
using Application.Services.Auth;
using Application.Services.System;
using Application.Services.Tenant;
using Application.Services.User;
using Domain.Entities.System;
using Domain.Entities.User;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories.System;
using Infrastructure.Persistence.Repositories.Tenant;
using Infrastructure.Persistence.Repositories.User;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Email;
using Infrastructure.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>Registers all Infrastructure-layer services with the DI container.</summary>
public static class DependencyInjection
{
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
        services.AddScoped<ISystemOwnerRepository, SystemOwnerRepository>();
        services.AddScoped<ISystemOwnerSessionRepository, SystemOwnerSessionRepository>();
        services.AddScoped<ISystemOwnerAuditLogRepository, SystemOwnerAuditLogRepository>();

        // -------------------------------------------------------------------------
        // Identity
        // -------------------------------------------------------------------------

        services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
        services.AddScoped<IPasswordHasher<SystemOwnerEntity>, PasswordHasher<SystemOwnerEntity>>();

        // -------------------------------------------------------------------------
        // Services
        // -------------------------------------------------------------------------

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ISystemOwnerAuthService, SystemOwnerAuthService>();
        services.AddScoped<ISystemOwnerService, SystemOwnerService>();
        services.AddScoped<ITenantManagementService, TenantManagementService>();
        services.AddSingleton<ITokenService, TokenService>();

        // -------------------------------------------------------------------------
        // Seeders
        // -------------------------------------------------------------------------

        services.AddScoped<DatabaseSeeder>();

        // -------------------------------------------------------------------------
        // Email
        // -------------------------------------------------------------------------

        services.AddSingleton<EmailBackgroundQueue>();
        services.AddSingleton<IEmailBackgroundQueue>(sp => sp.GetRequiredService<EmailBackgroundQueue>());
        services.AddHostedService(sp => sp.GetRequiredService<EmailBackgroundQueue>());
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}
