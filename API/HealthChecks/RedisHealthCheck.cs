using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace API.HealthChecks;

/// <summary>
/// Pings Redis via the DI-registered IConnectionMultiplexer singleton.
/// Only registered when Redis is configured (ConnectionStrings:Redis is non-empty).
/// </summary>
public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await redis.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy("Redis reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis unreachable", ex);
        }
    }
}
