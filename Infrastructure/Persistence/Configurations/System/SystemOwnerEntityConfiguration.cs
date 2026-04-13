using Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.System;

public sealed class SystemOwnerEntityConfiguration : IEntityTypeConfiguration<SystemOwnerEntity>
{
    public void Configure(EntityTypeBuilder<SystemOwnerEntity> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();
        builder.Property(o => o.RowVersion).IsRowVersion();

        builder.HasIndex(o => o.Email).IsUnique();
        builder.HasIndex(o => o.ResetTokenHash);

        builder.Property(o => o.Email).HasMaxLength(256).IsRequired();
        builder.Property(o => o.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(o => o.ResetTokenHash).HasMaxLength(128);

        builder.HasMany(o => o.Sessions)
            .WithOne(s => s.SystemOwner)
            .HasForeignKey(s => s.SystemOwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.AuditLogs)
            .WithOne(a => a.SystemOwner)
            .HasForeignKey(a => a.SystemOwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
