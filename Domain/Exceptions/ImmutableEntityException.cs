namespace Domain.Exceptions;

public class ImmutableEntityException : Exception
{
    public ImmutableEntityException(string entityName) : base($"{entityName} is immutable and cannot be modified or deleted.") { }
    public ImmutableEntityException(string entityName, string reason) : base($"{entityName} is immutable and cannot be modified or deleted. Reason: {reason}") { }
}