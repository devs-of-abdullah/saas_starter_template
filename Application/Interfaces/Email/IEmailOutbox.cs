namespace Application.Interfaces.Email;

/// <summary>
/// Stages outgoing transactional emails in the current Unit of Work scope.
/// Emails are persisted atomically with the calling business operation and
/// delivered by <c>OutboxEmailProcessor</c>.
/// </summary>
public interface IEmailOutbox
{
    void AddVerification(string to, string code);
    void AddPasswordReset(string to, string code);
    void AddEmailChange(string to, string code);
}
