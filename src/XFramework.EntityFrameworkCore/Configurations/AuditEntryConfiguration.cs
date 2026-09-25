using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Auditing;

namespace XFramework.EntityFrameworkCore.Configurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasMaxLength(128);
        builder.Property(x => x.UserName).HasMaxLength(256);
        builder.Property(x => x.EntityName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.ChangesJson).HasColumnType("NVARCHAR(MAX)");

        builder.HasIndex(x => new { x.EntityName, x.EntityId });
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => x.UserId);
    }
}