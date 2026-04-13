using Application.Interfaces.Email;
using Domain.Entities.Email;
using Domain.Enums.Email;
using Infrastructure.Persistence;

namespace Infrastructure.Services.Email;

/// <summary>
/// Scoped service that stages outgoing emails in the current DbContext change tracker.
/// Emails are committed atomically when the caller invokes SaveChangesAsync,
/// then delivered asynchronously by <see cref="OutboxEmailProcessor"/>.
/// </summary>
public sealed class EmailOutbox : IEmailOutbox
{
    readonly AppDbContext _context;

    public EmailOutbox(AppDbContext context)
    {
        _context = context;
    }

    public void AddVerification(string to, string code) =>
        Stage(EmailKind.Verification, to, code);

    public void AddPasswordReset(string to, string code) =>
        Stage(EmailKind.PasswordReset, to, code);

    public void AddEmailChange(string to, string code) =>
        Stage(EmailKind.EmailChange, to, code);

    void Stage(EmailKind kind, string to, string code) =>
        _context.OutboxEmails.Add(new OutboxEmailEntity
        {
            Kind      = kind,
            To        = to,
            Code      = code,
            CreatedAt = DateTimeOffset.UtcNow
        });
}
