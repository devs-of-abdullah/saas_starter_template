using API.Middleware;

namespace Microsoft.AspNetCore.Builder;

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder) => builder.UseMiddleware<SecurityHeadersMiddleware>();
}
