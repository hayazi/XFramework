using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace XFramework.EntityFrameworkCore.Idempotency;

public sealed class ProcessedMessageConfiguration
    : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(
        EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("ProcessedMessages");

        builder.HasKey(x => new
        {
            x.EventId,
            x.HandlerName
        });

        builder.Property(x => x.HandlerName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);
    }
}