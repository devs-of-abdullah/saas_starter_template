using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.TenantId, a.CreatedAt }); 
        builder.HasIndex(a => a.Action);

        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(100).IsRequired();

        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(1024);

        builder.HasOne(a => a.User).WithMany(u => u.AuditLogs).HasForeignKey(a => a.UserId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Tenant).WithMany().HasForeignKey(a => a.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}