using API.Models;
using RedisRateLimiting;
using StackExchange.Redis;
using System.Threading.RateLimiting;

namespace API.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddCustomRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Both limiters use AddPolicy so they can partition by client IP.
            // At request time we resolve the DI-registered IConnectionMultiplexer
            // singleton (registered by ServiceExtensions when Redis is configured)
            // and fall back to an in-process fixed-window limiter when Redis is absent.

            options.AddPolicy("AuthLimiter", httpContext =>
            {
                string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                IConnectionMultiplexer? redis = httpContext.RequestServices
                    .GetService<IConnectionMultiplexer>();

                if (redis is not null)
                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                        ip,
                        _ => new RedisSlidingWindowRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () => redis,
                            PermitLimit = 10,
                            Window      = TimeSpan.FromMinutes(1),
                        });

                return RateLimitPartition.GetFixedWindowLimiter(ip,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window      = TimeSpan.FromMinutes(1),
                        QueueLimit  = 0,
                    });
            });

            options.AddPolicy("GeneralLimiter", httpContext =>
            {
                string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                IConnectionMultiplexer? redis = httpContext.RequestServices
                    .GetService<IConnectionMultiplexer>();

                if (redis is not null)
                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                        ip,
                        _ => new RedisSlidingWindowRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () => redis,
                            PermitLimit = 100,
                            Window      = TimeSpan.FromMinutes(1),
                        });

                return RateLimitPartition.GetFixedWindowLimiter(ip,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window      = TimeSpan.FromMinutes(1),
                        QueueLimit  = 10,
                    });
            });

            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode  = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                // Inform clients how long to wait before retrying.
                // The sliding window is 60 s, so 60 is a safe upper bound.
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();
                else
                    context.HttpContext.Response.Headers.RetryAfter = "60";

                await context.HttpContext.Response.WriteAsJsonAsync(
                    ApiResponse.Error(
                        StatusCodes.Status429TooManyRequests,
                        "Too many requests. Please try again later."),
                    ct);
            };
        });

        return services;
    }
}
