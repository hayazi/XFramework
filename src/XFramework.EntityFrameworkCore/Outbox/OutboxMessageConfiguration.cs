using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace XFramework.EntityFrameworkCore.Outbox;
public sealed class OutboxMessageConfiguration:IEntityTypeConfiguration<OutboxMessage>
{ public void Configure(EntityTypeBuilder<OutboxMessage> b){b.ToTable("OutboxMessages");b.HasKey(x=>x.Id);b.Property(x=>x.EventType).HasMaxLength(500).IsRequired();b.Property(x=>x.EventVersion).IsRequired();b.Property(x=>x.Payload).IsRequired();b.Property(x=>x.Status).IsRequired();b.Property(x=>x.Headers).HasMaxLength(4000);b.Property(x=>x.LastError).HasMaxLength(4000);b.Property(x=>x.CorrelationId).HasMaxLength(100);b.Property(x=>x.CausationId).HasMaxLength(100);b.Property(x=>x.AggregateType).HasMaxLength(200);b.Property(x=>x.AggregateId).HasMaxLength(100);b.Property(x=>x.ModuleName).HasMaxLength(100);b.Property(x=>x.LockId).HasMaxLength(100);b.HasIndex(x=>new{x.Status,x.NextAttemptOnUtc,x.LockedUntilUtc});b.HasIndex(x=>x.EventId).IsUnique();} }
