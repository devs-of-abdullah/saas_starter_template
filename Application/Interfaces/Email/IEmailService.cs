namespace Application.Interfaces.Email;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string to, string code, CancellationToken ct = default);
    Task SendPasswordResetAsync(string to, string code, CancellationToken ct = default);
    Task SendEmailChangeAsync(string to, string code, CancellationToken ct = default);
}
