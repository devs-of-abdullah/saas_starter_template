using System.Security.Cryptography;
using System.Text;

namespace Application.Common;


internal static class CryptoHelpers
{
   
    internal static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }
    internal static string GenerateSecureCode() => RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
    internal static string GenerateSecureToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}