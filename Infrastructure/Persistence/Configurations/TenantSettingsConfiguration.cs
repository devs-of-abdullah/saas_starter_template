using Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettingsEntity>
{
    public void Configure(EntityTypeBuilder<TenantSettingsEntity> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => t.TenantId).IsUnique();

        builder.Property(t => t.Name).HasMaxLength(256).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LogoUrl).HasMaxLength(2048);
        builder.Property(t => t.FaviconUrl).HasMaxLength(2048);
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.PrimaryColor).HasMaxLength(7);
        builder.Property(t => t.SecondaryColor).HasMaxLength(7);
        builder.Property(t => t.SmtpHost).HasMaxLength(256);
        builder.Property(t => t.SmtpSenderEmail).HasMaxLength(256);
        builder.Property(t => t.SmtpSenderPasswordEncrypted).HasMaxLength(1024);
    }

}