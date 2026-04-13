using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Background;

/// <summary>
/// Daily background job that deletes expired and long-revoked sessions, and
/// removes processed outbox emails, to prevent unbounded table growth.
/// </summary>
public sealed class SessionCleanupService : BackgroundService
{
    readonly IServiceScopeFactory _scopeFactory;
    readonly ILogger<SessionCleanupService> _logger;

    static readonly TimeSpan RunInterval     = TimeSpan.FromHours(24);
    // Sessions expired/revoked longer than 30 days ago are safe to delete.
    static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

    public SessionCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<SessionCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    // Wait before the first run so the app can finish starting up and accept
    // real traffic before we issue a heavy DELETE against the database.
    static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);

            try { await Task.Delay(RunInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    async Task CleanupAsync(CancellationToken ct)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            DateTimeOffset cutoff = DateTimeOffset.UtcNow.Subtract(RetentionPeriod);

            // User sessions: expired or revoked before the retention window
            int userSessions = await db.UserSessions
                .Where(s => s.RefreshTokenExpiresAt < cutoff
                         || (s.RefreshTokenRevokedAt != null && s.RefreshTokenRevokedAt < cutoff))
                .ExecuteDeleteAsync(ct);

            // SystemOwner sessions: same rule
            int systemSessions = await db.SystemOwnerSessions
                .Where(s => s.RefreshTokenExpiresAt < cutoff
                         || (s.RefreshTokenRevokedAt != null && s.RefreshTokenRevokedAt < cutoff))
                .ExecuteDeleteAsync(ct);

            // Outbox emails: successfully processed and older than retention window.
            int processedEmails = await db.OutboxEmails
                .Where(e => e.ProcessedAt != null && e.ProcessedAt < cutoff)
                .ExecuteDeleteAsync(ct);

            // Outbox emails: permanently failed (all retries exhausted) and older than
            // retention window. FailedAt is indexed with a partial filter for efficiency.
            int failedEmails = await db.OutboxEmails
                .Where(e => e.FailedAt != null && e.FailedAt < cutoff)
                .ExecuteDeleteAsync(ct);

            int outboxEmails = processedEmails + failedEmails;

            if (userSessions + systemSessions + outboxEmails > 0)
                _logger.LogInformation(
                    "Cleanup removed {UserSessions} user sessions, {SystemSessions} system-owner sessions, " +
                    "{ProcessedEmails} processed outbox emails, {FailedEmails} failed outbox emails",
                    userSessions, systemSessions, processedEmails, failedEmails);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "SessionCleanupService failed");
        }
    }
}
