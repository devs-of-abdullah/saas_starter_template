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
using Infrastructure.Services.Background;
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

        // Allow SMTP_PASSWORD environment variable to override the config value so that
        // the password can be injected at runtime without touching appsettings files.
        services.PostConfigure<Application.Settings.Email.EmailSettings>(opts =>
        {
            string? envPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            if (!string.IsNullOrWhiteSpace(envPassword))
                opts.SmtpPassword = envPassword;
        });

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
        // Email — outbox pattern
        //   EmailOutbox     : scoped writer; stages emails in the current UoW scope
        //   OutboxEmailProcessor : background delivery with retry + lease-based
        //                          distributed claim
        // -------------------------------------------------------------------------

        services.AddScoped<IEmailOutbox, EmailOutbox>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddHostedService<OutboxEmailProcessor>();

        // -------------------------------------------------------------------------
        // Background maintenance
        // -------------------------------------------------------------------------

        services.AddHostedService<SessionCleanupService>();

        return services;
    }
}
