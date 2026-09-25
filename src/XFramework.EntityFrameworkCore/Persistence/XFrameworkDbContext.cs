using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;
using XFramework.Domain.Auditing;
using XFramework.EntityFrameworkCore.Idempotency;
using XFramework.EntityFrameworkCore.Outbox;

namespace XFramework.EntityFrameworkCore.Persistence;

public sealed class XFrameworkDbContext(
    DbContextOptions<XFrameworkDbContext> options,
    IEventTypeRegistry eventTypeRegistry)
    : DomainEventDbContext(options, eventTypeRegistry)
{
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(XFrameworkDbContext).Assembly);
    }
}
