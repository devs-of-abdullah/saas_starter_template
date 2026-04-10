namespace Domain.Entities.Common;

public abstract class ImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}