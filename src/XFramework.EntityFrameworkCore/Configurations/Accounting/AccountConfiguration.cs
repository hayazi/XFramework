using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;
using XFramework.EntityFrameworkCore.ValueConverters;
using System.Text.Json;
using System.Collections.Generic;

namespace XFramework.EntityFrameworkCore.Configurations.Accounting;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

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

        builder.Property(x => x.Nature)
            .IsRequired();

        builder.Property(x => x.Currency)
            .IsRequired();

        builder.Property(x => x.ParentAccountId);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDetail)
            .IsRequired();

        builder.HasOne(x => x.ParentAccount)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.ParentAccountId);
    }
}

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reference)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Reference)
            .IsUnique();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.PostingStatus)
            .IsRequired();

        builder.Property(x => x.PartyId);

        builder.Property(x => x.PostedOnUtc);

        builder.Property(x => x.PostedBy);

        var linesConverter = new ValueConverter<IReadOnlyCollection<JournalLine>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<JournalLine>>(v, (JsonSerializerOptions?)null)!.AsReadOnly());

        var linesComparer = new ValueComparer<IReadOnlyCollection<JournalLine>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2, new JournalLineEqualityComparer()),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.Select(l => new JournalLine(l.AccountId, l.Side, l.Amount, l.Description, l.DimensionValueId)).ToList().AsReadOnly());

        builder.Property(x => x.Lines)
            .HasConversion(linesConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .Metadata.SetValueComparer(linesComparer);

        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PostingStatus);
        builder.HasIndex(x => x.PartyId);
        builder.HasIndex(x => x.Reference);
    }
}

public class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("FiscalPeriods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.PeriodNumber)
            .IsRequired();

        builder.HasIndex(x => new { x.Year, x.PeriodNumber })
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        var dateRangeConverter = new ValueConverter<DateRange, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<DateRange>(v, (JsonSerializerOptions?)null)!);

        var dateRangeComparer = new ValueComparer<DateRange>(
            (c1, c2) => c1 != null && c2 != null && c1.Start == c2.Start && c1.End == c2.End,
            c => HashCode.Combine(c.Start, c.End),
            c => new DateRange(c.Start, c.End));

        builder.Property(x => x.DateRange)
            .HasConversion(dateRangeConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .Metadata.SetValueComparer(dateRangeComparer);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ClosedOnUtc);

        builder.Property(x => x.ClosedBy);

        builder.HasIndex(x => x.Year);
        builder.HasIndex(x => x.Status);
        // builder.HasIndex(x => x.DateRange); // Cannot index nvarchar(max) JSON column
    }
}

internal class JournalLineEqualityComparer : IEqualityComparer<JournalLine>
{
    public bool Equals(JournalLine x, JournalLine y)
    {
        if (x.Equals(y)) return true;
        return x.AccountId == y.AccountId
            && x.Side == y.Side
            && x.Amount.Equals(y.Amount)
            && x.Description == y.Description
            && x.DimensionValueId == y.DimensionValueId;
    }

    public int GetHashCode(JournalLine obj)
    {
        return HashCode.Combine(obj.AccountId, obj.Side, obj.Amount, obj.Description, obj.DimensionValueId);
    }
}