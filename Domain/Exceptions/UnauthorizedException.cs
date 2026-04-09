namespace Domain.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "You are not authenticated.") : base(message) { }
}