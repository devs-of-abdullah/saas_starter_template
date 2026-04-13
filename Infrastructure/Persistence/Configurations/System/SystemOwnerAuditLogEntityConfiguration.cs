using Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.System;

public sealed class SystemOwnerAuditLogEntityConfiguration : IEntityTypeConfiguration<SystemOwnerAuditLogEntity>
{
    public void Configure(EntityTypeBuilder<SystemOwnerAuditLogEntity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.HasIndex(a => a.SystemOwnerId);
        builder.HasIndex(a => new { a.SystemOwnerId, a.CreatedAt });

        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(100).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(512);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(512);
    }
}
