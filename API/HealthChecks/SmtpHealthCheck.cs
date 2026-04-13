using Application.Settings.Email;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace API.HealthChecks;

/// <summary>
/// Verifies that the SMTP server is reachable by opening a TCP connection.
/// Does not authenticate — it only confirms the port is up.
/// </summary>
public sealed class SmtpHealthCheck(IOptions<EmailSettings> options) : IHealthCheck
{
    readonly EmailSettings _settings = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, cancellationToken);
            return HealthCheckResult.Healthy($"SMTP reachable at {_settings.SmtpServer}:{_settings.SmtpPort}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"SMTP unreachable at {_settings.SmtpServer}:{_settings.SmtpPort}",
                ex);
        }
    }
}
