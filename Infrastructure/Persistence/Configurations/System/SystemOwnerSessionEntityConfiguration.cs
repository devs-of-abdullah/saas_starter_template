using Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.System;

public sealed class SystemOwnerSessionEntityConfiguration : IEntityTypeConfiguration<SystemOwnerSessionEntity>
{
    public void Configure(EntityTypeBuilder<SystemOwnerSessionEntity> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasIndex(s => s.RefreshTokenHash).IsUnique();
        builder.HasIndex(s => s.SystemOwnerId);

        builder.Property(s => s.RefreshTokenHash).HasMaxLength(128).IsRequired();
        builder.Property(s => s.IpAddress).HasMaxLength(45);
        builder.Property(s => s.UserAgent).HasMaxLength(512);
        builder.Property(s => s.DeviceInfo).HasMaxLength(512);
    }
}
