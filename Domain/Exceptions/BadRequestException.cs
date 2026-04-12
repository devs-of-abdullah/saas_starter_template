namespace Domain.Exceptions;

public sealed class BadRequestException : DomainException
{
    public BadRequestException(string message) : base(message) { }
}
