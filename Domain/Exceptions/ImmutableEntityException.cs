namespace Domain.Exceptions;

public sealed class ImmutableEntityException : DomainException
{
    public ImmutableEntityException(string entityName) : base($"{entityName} is immutable and cannot be modified or deleted.") { }

    public ImmutableEntityException(string entityName, string reason) : base($"{entityName} is immutable and cannot be modified or deleted. Reason: {reason}") { }
}
