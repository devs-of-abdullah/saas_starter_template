using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Common;
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>(); // to handle concurrency between multiple instances of the application
}