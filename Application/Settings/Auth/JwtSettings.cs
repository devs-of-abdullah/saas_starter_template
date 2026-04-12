namespace Application.Settings.Auth;

public sealed class JwtSettings
{
    public string PrivateKeyPem { get; init; } = null!;
    public string PublicKeyPem { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public int ExpiresInMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;
}
