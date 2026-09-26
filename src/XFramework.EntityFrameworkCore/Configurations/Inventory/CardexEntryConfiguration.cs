using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;
using XFramework.EntityFrameworkCore.ValueConverters;
using System.Text.Json;
using System.Collections.Generic;

namespace XFramework.EntityFrameworkCore.Configurations.Inventory;

public class CardexEntryConfiguration : IEntityTypeConfiguration<CardexEntry>
{
    public void Configure(EntityTypeBuilder<CardexEntry> builder)
    {
        builder.ToTable("CardexEntries");

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

        var quantityConverter = new ValueConverter<Quantity, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Quantity>(v, (JsonSerializerOptions?)null)!);

        var quantityComparer = new ValueComparer<Quantity>(
            (c1, c2) => c1 != null && c2 != null && c1.Value == c2.Value && c1.Unit == c2.Unit,
            c => HashCode.Combine(c.Value, c.Unit),
            c => new Quantity(c.Value, c.Unit));

        builder.Property(x => x.QuantityIn)
            .HasConversion(quantityConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .Metadata.SetValueComparer(quantityComparer);

        builder.Property(x => x.QuantityOut)
            .HasConversion(quantityConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .Metadata.SetValueComparer(quantityComparer);

        builder.Property(x => x.RunningBalance)
            .HasConversion(quantityConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .Metadata.SetValueComparer(quantityComparer);

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

        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.TransactionDate);
        builder.HasIndex(x => x.DocumentReference);
        builder.HasIndex(x => x.TransactionType);
        builder.HasIndex(x => x.ReferenceDocumentId);
    }
}