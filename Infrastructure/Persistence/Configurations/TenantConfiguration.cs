
using Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class TenantConfiguration : IEntityTypeConfiguration<TenantEntity>
    {
        public void Configure(EntityTypeBuilder<TenantEntity> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.Property(t => t.RowVersion).IsRowVersion();

            builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.Property(t => t.Plan).HasConversion<string>().HasMaxLength(50).IsRequired();

            builder.HasOne(t => t.Settings).WithOne(s => s.Tenant).HasForeignKey<TenantSettingsEntity>(s => s.TenantId) .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Users).WithOne(u => u.Tenant).HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
