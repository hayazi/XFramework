using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(
        EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.EventVersion)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(4000);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder.Property(x => x.CausationId)
            .HasMaxLength(100);

        builder.Property(x => x.AggregateType)
            .HasMaxLength(200);

        builder.Property(x => x.AggregateId)
            .HasMaxLength(100);

        builder.Property(x => x.LockId)
            .HasMaxLength(100);

        builder.HasIndex(x => new
        {
            x.Status,
            x.NextAttemptOnUtc,
            x.LockedUntilUtc
        });
        builder.HasIndex(x => new
        {
            x.EventType,
            x.EventVersion
        });
    }
}