using Application.Constants;
using Application.Interfaces.Auth;
using Application.Settings.Auth;
using Application.Settings.Email;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace API.Extensions;

public static class ServiceExtensions
{
  
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration configuration)
    {

        ValidateRequiredSettings(configuration);

     
        string connectionString = Environment.GetEnvironmentVariable("DefaultConnection") ?? configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Database connection string not found.");

        services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Infrastructure")));
        services.AddAuthInfrastructure(configuration);
        services.AddHttpContextAccessor();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<IAuthService>();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });



        services.AddControllers().AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                x.JsonSerializerOptions.WriteIndented = true;
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

 

        services.AddCors(options =>
        {
            string[]? allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

            bool isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

            options.AddPolicy(ClaimConstants.CorsPolicyName, policy =>
            {
                if (allowedOrigins is { Length: > 0 })
                {
                    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
                }
                else if (!isProduction)
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                }
                else
                {
                    throw new InvalidOperationException("AllowedOrigins must be configured in Production.");
                }
            });
        }); 

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

        return services;
    }

    static void ValidateRequiredSettings(IConfiguration configuration)
    {
        JwtSettings jwt = new();
        configuration.GetSection("JwtSettings").Bind(jwt);

        if (string.IsNullOrWhiteSpace(jwt.PrivateKeyPem) ||
            jwt.PrivateKeyPem.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("JwtSettings:PrivateKeyPem is not configured. " + "Set the JWT_PRIVATE_KEY_PEM environment variable or appsettings override.");

        if (string.IsNullOrWhiteSpace(jwt.PublicKeyPem) ||
            jwt.PublicKeyPem.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("JwtSettings:PublicKeyPem is not configured. " + "Set the JWT_PUBLIC_KEY_PEM environment variable or appsettings override.");

        if (string.IsNullOrWhiteSpace(jwt.Issuer))
            throw new InvalidOperationException("JwtSettings:Issuer is not configured.");

        if (string.IsNullOrWhiteSpace(jwt.Audience))
            throw new InvalidOperationException("JwtSettings:Audience is not configured.");

        EmailSettings email = new();
        configuration.GetSection("EmailSettings").Bind(email);

        if (string.IsNullOrWhiteSpace(email.SmtpPassword) || email.SmtpPassword.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("EmailSettings:SmtpPassword is not configured. " + "Set the SMTP_PASSWORD environment variable or appsettings override.");
    }
}
