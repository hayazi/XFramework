using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Inventory;
using XFramework.EntityFrameworkCore.ValueConverters;

namespace XFramework.EntityFrameworkCore.Configurations.Inventory;

public class KardexEntryConfiguration : IEntityTypeConfiguration<KardexEntry>
{
    public void Configure(EntityTypeBuilder<KardexEntry> builder)
    {
        builder.ToTable("KardexEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.TransactionType)
            .IsRequired();

        builder.Property(x => x.DocumentReference)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TransactionDate)
            .IsRequired();

        builder.Property(x => x.QuantityIn)
            .HasConversion(new QuantityConverter())
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.QuantityOut)
            .HasConversion(new QuantityConverter())
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.RunningBalance)
            .HasConversion(new QuantityConverter())
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.UnitCost)
            .HasConversion(new MoneyConverter())
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.TotalCost)
            .HasConversion(new MoneyConverter())
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.CostingMethod)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ReferenceDocumentId);

        builder.Property(x => x.ReferenceDocumentLine);

        builder.HasIndex(x => new { x.ItemId, x.WarehouseId, x.TransactionDate });
        builder.HasIndex(x => x.TransactionType);
        builder.HasIndex(x => x.DocumentReference);
        builder.HasIndex(x => x.ReferenceDocumentId);
    }
}