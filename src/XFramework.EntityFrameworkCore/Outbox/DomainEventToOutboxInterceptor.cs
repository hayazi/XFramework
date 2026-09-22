using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using XFramework.Application.Events;
using XFramework.Domain.Events;

namespace XFramework.EntityFrameworkCore.Outbox;

/// <summary>
/// Converts tracked domain events into OutboxMessage entities during the
/// EF Core SaveChanges pipeline. The outbox rows are saved by the same
/// SaveChanges call and therefore participate in the caller's transaction.
/// Domain events are cleared only after a successful save.
/// </summary>
public sealed class DomainEventToOutboxInterceptor : SaveChangesInterceptor
{
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly List<IHasDomainEvents> _preparedEntities = new();

    public DomainEventToOutboxInterceptor(IEventTypeRegistry eventTypeRegistry)
    {
        _eventTypeRegistry = eventTypeRegistry
            ?? throw new ArgumentNullException(nameof(eventTypeRegistry));
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        PrepareDomainEvents(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        PrepareDomainEvents(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        ClearPreparedDomainEvents();
        return result;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        ClearPreparedDomainEvents();
        return ValueTask.FromResult(result);
    }

    public override void SaveChangesFailed(
        DbContextErrorEventData eventData)
    {
        // Keep domain events on their entities so a failed save can be
        // inspected or retried. Only discard the interceptor's bookkeeping.
        _preparedEntities.Clear();
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _preparedEntities.Clear();
        return Task.CompletedTask;
    }

    private void PrepareDomainEvents(DbContext? context)
    {
        if (context is null)
            return;

        context.ChangeTracker.DetectChanges();
        _preparedEntities.Clear();

        var entries = context.ChangeTracker
            .Entries()
            .Where(x => x.Entity is IHasDomainEvents hasDomainEvents
                        && hasDomainEvents.DomainEvents.Count > 0)
            .ToList();

        if (entries.Count == 0)
            return;

        var preparedEventIds = new HashSet<Guid>();
        var outbox = context.Set<OutboxMessage>();

        foreach (var entry in entries)
        {
            var entityWithDomainEvents = (IHasDomainEvents)entry.Entity;
            _preparedEntities.Add(entityWithDomainEvents);

            var primaryKey = entry.Metadata.FindPrimaryKey();
            string? aggregateId = null;

            if (primaryKey is not null && primaryKey.Properties.Count == 1)
            {
                aggregateId = entry.Property(primaryKey.Properties[0].Name)
                    .CurrentValue?.ToString();
            }

            foreach (var domainEvent in entityWithDomainEvents.DomainEvents)
            {
                if (!preparedEventIds.Add(domainEvent.EventId))
                    continue;

                var type = domainEvent.GetType();
                var eventType = _eventTypeRegistry.GetEventTypeName(type);
                var version = _eventTypeRegistry.GetEventVersion(type);

                outbox.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventId = domainEvent.EventId,
                    EventType = eventType,
                    EventVersion = version,
                    Payload = JsonSerializer.Serialize(domainEvent, type),
                    Status = OutboxMessageStatus.Pending,
                    RetryCount = 0,
                    CreatedOnUtc = DateTime.UtcNow,
                    OccurredOnUtc = domainEvent.OccurredOnUtc,
                    CorrelationId = domainEvent.CorrelationId?.ToString(),
                    CausationId = domainEvent.CausationId?.ToString(),
                    AggregateType = entry.Entity.GetType().Name,
                    AggregateId = aggregateId
                });
            }
        }
    }

    private void ClearPreparedDomainEvents()
    {
        foreach (var entity in _preparedEntities)
        {
            if (entity.DomainEvents.Count > 0)
                entity.ClearDomainEvents();
        }

        _preparedEntities.Clear();
    }
}
