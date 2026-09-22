using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;

namespace XFramework.EntityFrameworkCore.Outbox;

/// <summary>
/// DbContext base that exposes the transactional domain-event-to-outbox
/// behavior. Concrete applications can register the interceptor through DI;
/// OnConfiguring also provides a safe default for directly-created contexts.
/// </summary>
public abstract class DomainEventDbContext : DbContext
{
    private readonly IEventTypeRegistry _eventTypeRegistry;

    protected DomainEventDbContext(
        DbContextOptions options,
        IEventTypeRegistry eventTypeRegistry)
        : base(options)
    {
        _eventTypeRegistry = eventTypeRegistry
            ?? throw new ArgumentNullException(nameof(eventTypeRegistry));
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // A directly-created DbContext (for example, integration tests) must
        // still have the outbox interceptor. DI-based production contexts
        // may register the same interceptor explicitly in AddDbContext.
        optionsBuilder.AddInterceptors(
            new DomainEventToOutboxInterceptor(_eventTypeRegistry));

        base.OnConfiguring(optionsBuilder);
    }
}
