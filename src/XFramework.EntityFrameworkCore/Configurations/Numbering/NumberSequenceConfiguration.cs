using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Numbering;

namespace XFramework.EntityFrameworkCore.Configurations.Numbering;

public class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> builder)
    {
        builder.ToTable("NumberSequences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Prefix)
            .HasMaxLength(50);

        builder.Property(x => x.Suffix)
            .HasMaxLength(50);

        builder.Property(x => x.CurrentNumber)
            .IsRequired();

        builder.Property(x => x.MinimumDigits)
            .IsRequired();

        builder.Property(x => x.IncrementBy)
            .IsRequired();

        builder.Property(x => x.MaximumNumber);

        builder.Property(x => x.Scope)
            .IsRequired();

        builder.Property(x => x.ScopeIdentifier)
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ResetDate);

        builder.Property(x => x.AutoReset)
            .IsRequired();

        builder.Property(x => x.FormatTemplate)
            .HasMaxLength(200);

        builder.HasIndex(x => new { x.Code, x.Scope, x.ScopeIdentifier })
            .IsUnique();
        
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Scope);
    }
}