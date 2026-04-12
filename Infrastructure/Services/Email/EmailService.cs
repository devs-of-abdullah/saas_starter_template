using Application.Interfaces.Email;
using Application.Settings.Email;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services.Email;

public sealed class EmailService : IEmailService
{
    readonly EmailSettings _settings;
    readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string to, string code, CancellationToken ct = default) => SendAsync(to, "Verify your email", EmailTemplates.VerificationEmail(code), ct);

    public Task SendPasswordResetAsync(string to, string code, CancellationToken ct = default) => SendAsync(to, "Reset your password", EmailTemplates.PasswordResetEmail(code), ct);

    public Task SendEmailChangeAsync(string to, string code, CancellationToken ct = default) => SendAsync(to, "Confirm your new email address", EmailTemplates.EmailChangeEmail(code), ct);

    private async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort,
            SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, ct);
            await client.SendAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", to, subject);
            throw;
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(quit: true, ct);
        }
    }
}