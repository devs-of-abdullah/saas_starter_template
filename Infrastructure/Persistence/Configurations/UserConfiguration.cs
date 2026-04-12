using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();
        builder.Property(u => u.RowVersion).IsRowVersion();

        builder.HasIndex(u => new { u.Email, u.TenantId }).IsUnique();
        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.Role);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(u => u.ResetTokenHash).HasMaxLength(512);
        builder.Property(u => u.EmailVerificationTokenHash).HasMaxLength(512);
        builder.Property(u => u.PendingEmail).HasMaxLength(256);
        builder.Property(u => u.PendingEmailTokenHash).HasMaxLength(512);

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.HasMany(u => u.Sessions).WithOne(s => s.User).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AuditLogs).WithOne(a => a.User).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}