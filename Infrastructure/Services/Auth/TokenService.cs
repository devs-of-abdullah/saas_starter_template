using Application.Constants;
using Application.Interfaces.Auth;
using Application.Settings.Auth;
using Domain.Entities.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Infrastructure.Services.Auth;

public sealed class TokenService : ITokenService
{
    readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateAccessToken(UserEntity user)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(_settings.PrivateKeyPem);

        SigningCredentials credentials = new(new RsaSecurityKey(rsa) { KeyId = "primary" }, SecurityAlgorithms.RsaSha256);

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimConstants.TenantId, user.TenantId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        ];

        JwtSecurityToken token = new(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTimeOffset.UtcNow.AddMinutes(_settings.ExpiresInMinutes).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
