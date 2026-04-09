namespace Domain.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
    public ConflictException(string entity, string field, object value) : base($"{entity} with {field} '{value}' already exists.") { }
}