namespace Domain.Exceptions;

/// <summary>Thrown when a caller exceeds an allowed request rate.</summary>
public sealed class TooManyRequestsException : DomainException
{
    /// <summary>Initialises the exception with the default message.</summary>
    public TooManyRequestsException(string message = "Too many requests. Please try again later.") : base(message) { }
}
