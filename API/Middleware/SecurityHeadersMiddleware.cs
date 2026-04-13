using Microsoft.Extensions.Logging;

namespace API.Middleware;

public sealed class SecurityHeadersMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string requestId = context.TraceIdentifier;

        context.Response.Headers.Append("X-Request-ID",            requestId);
        context.Response.Headers.Append("X-Content-Type-Options",  "nosniff");
        context.Response.Headers.Append("X-Frame-Options",         "DENY");
        context.Response.Headers.Append("Referrer-Policy",         "no-referrer");
        context.Response.Headers.Append("X-XSS-Protection",        "1; mode=block");
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
        context.Response.Headers.Append("Permissions-Policy",      "geolocation=(), microphone=(), camera=()");

        // Push RequestId into the log scope so every log line within this request
        // is automatically annotated with the trace identifier.
        using (_logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = requestId }))
        {
            await _next(context);
        }
    }
}
