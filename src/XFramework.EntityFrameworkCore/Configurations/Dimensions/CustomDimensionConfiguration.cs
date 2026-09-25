using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XFramework.Domain.Dimensions;
using System.Text.Json;

namespace XFramework.EntityFrameworkCore.Configurations.Dimensions;

public class CustomDimensionConfiguration : IEntityTypeConfiguration<CustomDimension>
{
    public void Configure(EntityTypeBuilder<CustomDimension> builder)
    {
        builder.ToTable("CustomDimensions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.DimensionKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.DimensionKey);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .IsRequired();

        builder.Property(x => x.AllowHierarchy)
            .IsRequired();

        builder.Property(x => x.ParentDimensionId);

        builder.Property(x => x.Attributes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.ParentDimension)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentDimensionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ParentDimensionId);
        builder.HasIndex(x => x.DimensionKey);
    }
}