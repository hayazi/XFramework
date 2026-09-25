using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Inventory;
using XFramework.EntityFrameworkCore.ValueConverters;

namespace XFramework.EntityFrameworkCore.Configurations.Inventory;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

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

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.BaseUnit)
            .IsRequired();

        builder.Property(x => x.CostingMethod)
            .IsRequired();

        builder.Property(x => x.StandardCost)
            .HasConversion(new MoneyConverter())
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.IsStocked)
            .IsRequired();

        builder.Property(x => x.IsPurchasable)
            .IsRequired();

        builder.Property(x => x.IsSellable)
            .IsRequired();

        builder.Property(x => x.IsProducible)
            .IsRequired();

        builder.Property(x => x.MinimumStock)
            .HasPrecision(18, 4);

        builder.Property(x => x.MaximumStock)
            .HasPrecision(18, 4);

        builder.Property(x => x.ReorderPoint)
            .HasPrecision(18, 4);

        builder.Property(x => x.DefaultWarehouseId);

        builder.Property(x => x.Barcode)
            .HasMaxLength(100);

        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.DefaultWarehouseId);
    }
}