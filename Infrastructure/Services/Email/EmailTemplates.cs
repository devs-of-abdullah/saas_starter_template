namespace Infrastructure.Services.Email;

internal static class EmailTemplates
{
    public static string VerificationEmail(string code) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <h2>Verify your email</h2>
            <p>Your verification code is:</p>
            <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 20px;
                        background: #f4f4f4; border-radius: 8px; text-align: center; margin: 20px 0;">
                {code}
            </div>
            <p>This code expires in <strong>15 minutes</strong>.</p>
            <p>If you did not create an account, you can safely ignore this email.</p>
        </body>
        </html>
        """;

    public static string PasswordResetEmail(string code) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <h2>Reset your password</h2>
            <p>Your password reset code is:</p>
            <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 20px;
                        background: #f4f4f4; border-radius: 8px; text-align: center; margin: 20px 0;">
                {code}
            </div>
            <p>This code expires in <strong>1 hour</strong>.</p>
            <p>If you did not request a password reset, you can safely ignore this email.</p>
        </body>
        </html>
        """;

    public static string EmailChangeEmail(string code) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <h2>Confirm your new email address</h2>
            <p>Your confirmation code is:</p>
            <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 20px;
                        background: #f4f4f4; border-radius: 8px; text-align: center; margin: 20px 0;">
                {code}
            </div>
            <p>This code expires in <strong>1 hour</strong>.</p>
            <p>If you did not request an email change, please secure your account immediately.</p>
        </body>
        </html>
        """;
}