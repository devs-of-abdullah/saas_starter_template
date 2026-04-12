namespace Application.Settings.Email;

public sealed class EmailSettings
{
    public string SenderName { get; init; } = null!;
    public string SenderEmail { get; init; } = null!;
    public string SmtpServer { get; init; } = null!;
    public int SmtpPort { get; init; } = 587;
    public string SmtpUsername { get; init; } = null!;
    public string SmtpPassword { get; init; } = null!;
}
