using Application.Interfaces.Email;
using Domain.Entities.Email;
using Domain.Enums.Email;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Email;

/// <summary>
/// Background service that polls the outbox table and delivers pending emails.
/// Uses a lease-based claim (LockedUntil) to prevent double-delivery across
/// multiple instances.
/// </summary>
public sealed class OutboxEmailProcessor : BackgroundService
{
    readonly IServiceScopeFactory _scopeFactory;
    readonly ILogger<OutboxEmailProcessor> _logger;

    const int BatchSize   = 10;
    const int MaxAttempts = 3;

    static readonly TimeSpan PollInterval  = TimeSpan.FromSeconds(10);
    static readonly TimeSpan LockDuration  = TimeSpan.FromMinutes(5);

    public OutboxEmailProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxEmailProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken);

            try { await Task.Delay(PollInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    async Task ProcessBatchAsync(CancellationToken ct)
    {
        try
        {
            using IServiceScope scope        = _scopeFactory.CreateScope();
            AppDbContext          db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            IEmailService         emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            DateTimeOffset now       = DateTimeOffset.UtcNow;
            DateTimeOffset lockUntil = now.Add(LockDuration);

            // Step 1 — identify candidate IDs
            List<Guid> candidateIds = await db.OutboxEmails
                .Where(e => e.ProcessedAt == null
                         && e.Attempts    < MaxAttempts
                         && (e.LockedUntil == null || e.LockedUntil < now))
                .OrderBy(e => e.CreatedAt)
                .Take(BatchSize)
                .Select(e => e.Id)
                .ToListAsync(ct);

            if (candidateIds.Count == 0) return;

            // Step 2 — atomically claim candidates (increment Attempts + set lease)
            await db.OutboxEmails
                .Where(e => candidateIds.Contains(e.Id)
                         && e.ProcessedAt == null
                         && (e.LockedUntil == null || e.LockedUntil < now))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.LockedUntil, lockUntil)
                    .SetProperty(e => e.Attempts, e => e.Attempts + 1),
                ct);

            // Step 3 — load the claimed batch
            List<OutboxEmailEntity> batch = await db.OutboxEmails
                .Where(e => candidateIds.Contains(e.Id) && e.ProcessedAt == null)
                .ToListAsync(ct);

            foreach (OutboxEmailEntity email in batch)
                await DeliverAsync(email, emailService, db, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "OutboxEmailProcessor batch failed");
        }
    }

    async Task DeliverAsync(
        OutboxEmailEntity email,
        IEmailService emailService,
        AppDbContext db,
        CancellationToken ct)
    {
        try
        {
            Task send = email.Kind switch
            {
                EmailKind.Verification  => emailService.SendEmailVerificationAsync(email.To, email.Code, ct),
                EmailKind.PasswordReset => emailService.SendPasswordResetAsync(email.To, email.Code, ct),
                EmailKind.EmailChange   => emailService.SendEmailChangeAsync(email.To, email.Code, ct),
                _                       => Task.CompletedTask
            };

            await send;

            await db.OutboxEmails
                .Where(e => e.Id == email.Id)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(e => e.ProcessedAt, DateTimeOffset.UtcNow), ct);
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;

            bool isPermanentFailure = email.Attempts >= MaxAttempts;

            if (isPermanentFailure)
            {
                // All retry attempts exhausted — mark as permanently failed so it is
                // never picked up again and is visible to monitoring/alerting queries.
                _logger.LogCritical(ex,
                    "Email permanently failed after {MaxAttempts} attempts and will not be retried. " +
                    "Kind={Kind} To={To} Id={Id} LastError={LastError}",
                    MaxAttempts, email.Kind, email.To, email.Id, errorMessage);

                await db.OutboxEmails
                    .Where(e => e.Id == email.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.LastError,   errorMessage)
                        .SetProperty(e => e.LockedUntil, e => (DateTimeOffset?)null)
                        .SetProperty(e => e.FailedAt,    DateTimeOffset.UtcNow),
                    ct);
            }
            else
            {
                _logger.LogError(ex,
                    "Failed to deliver {Kind} email to {To} (attempt {Attempts}/{MaxAttempts})",
                    email.Kind, email.To, email.Attempts, MaxAttempts);

                // Release the lease so another attempt can be made after LockDuration.
                await db.OutboxEmails
                    .Where(e => e.Id == email.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.LastError,   errorMessage)
                        .SetProperty(e => e.LockedUntil, e => (DateTimeOffset?)null),
                    ct);
            }
        }
    }
}
