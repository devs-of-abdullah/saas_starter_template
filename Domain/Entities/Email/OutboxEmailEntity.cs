using Domain.Enums.Email;

namespace Domain.Entities.Email;

public sealed class OutboxEmailEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EmailKind Kind { get; set; }
    public string To { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }

    /// <summary>Lease expiry set by the processor that claimed this item. Prevents double-processing across instances.</summary>
    public DateTimeOffset? LockedUntil { get; set; }

    /// <summary>Number of delivery attempts made so far.</summary>
    public int Attempts { get; set; }

    public string? LastError { get; set; }

    /// <summary>
    /// Set when all <see cref="MaxAttempts"/> are exhausted and the email will
    /// never be retried.  Used for monitoring, alerting, and scheduled cleanup.
    /// </summary>
    public DateTimeOffset? FailedAt { get; set; }
}
