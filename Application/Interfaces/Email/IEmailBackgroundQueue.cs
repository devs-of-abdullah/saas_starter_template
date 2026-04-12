namespace Application.Interfaces.Email;


public interface IEmailBackgroundQueue
{
    void EnqueueVerification(string to, string code);
    void EnqueuePasswordReset(string to, string code);
    void EnqueueEmailChange(string to, string code);
}
