namespace Domain.Exceptions;
public sealed class TooManyRequestsException : DomainException
{ 
    public TooManyRequestsException(string message = "Too many requests. Please try again later.") : base(message) { }
}
