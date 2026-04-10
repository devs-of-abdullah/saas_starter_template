using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSessionEntity>
{
    public void Configure(EntityTypeBuilder<UserSessionEntity> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.RefreshTokenExpiresAt);
        builder.HasIndex(s => s.RefreshTokenRevokedAt);

        builder.Property(s => s.RefreshTokenHash).HasMaxLength(512).IsRequired();
        builder.Property(s => s.DeviceInfo).HasMaxLength(256);
        builder.Property(s => s.IpAddress).HasMaxLength(45);
        builder.Property(s => s.UserAgent).HasMaxLength(1024);

        builder.HasOne(s => s.Tenant).WithMany().HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}