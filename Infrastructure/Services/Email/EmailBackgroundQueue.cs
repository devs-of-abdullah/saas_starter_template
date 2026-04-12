using Application.Interfaces.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using Domain.Enums.Email;
namespace Infrastructure.Services.Email;

public sealed class EmailBackgroundQueue : BackgroundService, IEmailBackgroundQueue
{
    readonly Channel<EmailWorkItem> _channel;
    readonly IServiceScopeFactory _scopeFactory;
    readonly ILogger<EmailBackgroundQueue> _logger;

    const int MaxRetries = 3;
    const int ChannelCapacity = 1_000;

    public EmailBackgroundQueue(IServiceScopeFactory scopeFactory, ILogger<EmailBackgroundQueue> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _channel = Channel.CreateBounded<EmailWorkItem>(new BoundedChannelOptions(ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });
    }

   
    public void EnqueueVerification(string to, string code) => TryEnqueue(new EmailWorkItem(EmailKind.Verification, to, code));

    public void EnqueuePasswordReset(string to, string code) => TryEnqueue(new EmailWorkItem(EmailKind.PasswordReset, to, code));

    public void EnqueueEmailChange(string to, string code) => TryEnqueue(new EmailWorkItem(EmailKind.EmailChange, to, code));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (EmailWorkItem item in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            await SendWithRetryAsync(item, stoppingToken);
        }
    }


    void TryEnqueue(EmailWorkItem item)
    {
        if (!_channel.Writer.TryWrite(item))
            _logger.LogWarning("Email queue is full — dropped {Kind} email to {To}", item.Kind, item.To);
    }

    async Task SendWithRetryAsync(EmailWorkItem item, CancellationToken ct)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                Task sendTask = item.Kind switch
                {
                    EmailKind.Verification  => emailService.SendEmailVerificationAsync(item.To, item.Code),
                    EmailKind.PasswordReset => emailService.SendPasswordResetAsync(item.To, item.Code),
                    EmailKind.EmailChange   => emailService.SendEmailChangeAsync(item.To, item.Code),
                    _                       => Task.CompletedTask
                };

                await sendTask;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email send attempt {Attempt}/{MaxRetries} failed — kind={Kind} to={To}", attempt, MaxRetries, item.Kind, item.To);

                if (attempt < MaxRetries)
                {
                    TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 2 s, 4 s
                    await Task.Delay(delay, ct);
                }
            }
        }

        _logger.LogError("All {MaxRetries} attempts exhausted — dropping {Kind} email to {To}", MaxRetries, item.Kind, item.To);
    }
}


internal sealed record EmailWorkItem(EmailKind Kind, string To, string Code);
