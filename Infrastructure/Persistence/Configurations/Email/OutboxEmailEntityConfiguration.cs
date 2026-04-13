using Domain.Entities.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Email;

public sealed class OutboxEmailEntityConfiguration : IEntityTypeConfiguration<OutboxEmailEntity>
{
    public void Configure(EntityTypeBuilder<OutboxEmailEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // Primary polling index: unprocessed, unlocked emails ordered by age
        builder.HasIndex(e => new { e.ProcessedAt, e.LockedUntil, e.CreatedAt });

        builder.Property(e => e.Kind).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.To).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(512).IsRequired();
        builder.Property(e => e.LastError).HasMaxLength(2000);

        // Index for monitoring/cleanup of permanently-failed emails.
        builder.HasIndex(e => e.FailedAt).HasFilter("[FailedAt] IS NOT NULL");
    }
}
