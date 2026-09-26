using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using XFramework.Domain.Dimensions;
using System.Text.Json;
using System.Collections.Generic;

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

        var attributesConverter = new ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        var attributesComparer = new ValueComparer<Dictionary<string, string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        builder.Property(x => x.Attributes)
            .HasConversion(attributesConverter)
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(attributesComparer);

        builder.HasOne(x => x.ParentDimension)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentDimensionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ParentDimensionId);
        builder.HasIndex(x => x.DimensionKey);
    }
}