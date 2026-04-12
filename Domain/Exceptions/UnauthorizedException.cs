namespace Domain.Exceptions;

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "You are not authenticated.") : base(message) { }
}
