using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XFramework.Domain.Tax;
using XFramework.EntityFrameworkCore.ValueConverters;
using System.Text.Json;

namespace XFramework.EntityFrameworkCore.Configurations.Tax;

public class TaxCodeConfiguration : IEntityTypeConfiguration<TaxCode>
{
    public void Configure(EntityTypeBuilder<TaxCode> builder)
    {
        builder.ToTable("TaxCodes");

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

        builder.Property(x => x.TaxType)
            .IsRequired();

        builder.Property(x => x.CalculationMethod)
            .IsRequired();

        builder.Property(x => x.Application)
            .IsRequired();

        builder.Property(x => x.Rate)
            .HasConversion(new PercentageConverter())
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.FixedAmount)
            .HasConversion(new MoneyNullableConverter())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.PerUnit);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.Property(x => x.EffectiveTo);

        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.Property(x => x.IsRecoverable)
            .IsRequired();

        builder.Property(x => x.AccountId);

        var tierConverter = new ValueConverter<List<TaxTier>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<TaxTier>>(v, (JsonSerializerOptions?)null) ?? new List<TaxTier>());

        builder.Property(x => x.Tiers)
            .HasConversion(tierConverter)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.TaxType);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.EffectiveFrom);
        builder.HasIndex(x => x.IsDefault);
    }
}