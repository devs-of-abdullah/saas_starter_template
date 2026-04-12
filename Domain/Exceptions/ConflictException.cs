namespace Domain.Exceptions;

public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string entity, string field, object value) : base($"{entity} with {field} '{value}' already exists.") { }
}
