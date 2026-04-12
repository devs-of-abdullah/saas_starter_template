using Application.Settings.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((bearerOptions, jwtOptions) =>
            {
                var settings = jwtOptions.Value;

                var rsa = RSA.Create();
                rsa.ImportFromPem(settings.PublicKeyPem.AsSpan());

                bearerOptions.RequireHttpsMetadata = true;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}