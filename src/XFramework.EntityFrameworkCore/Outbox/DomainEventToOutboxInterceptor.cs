using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using XFramework.Domain.Events;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class DomainEventToOutboxInterceptor
    : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddDomainEventsToOutbox(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private static void AddDomainEventsToOutbox(
        DbContext? context)
    {
        if (context is null)
            return;

        var entities = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Count > 0)
            .ToList();

        foreach (var entry in entities)
        {
            var events = entry.Entity.DomainEvents.ToList();

            foreach (var domainEvent in events)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),

                    EventType = domainEvent
                        .GetType()
                        .AssemblyQualifiedName!,

                    Payload = JsonSerializer.Serialize(
                        domainEvent,
                        domainEvent.GetType(),
                        JsonOptions),

                    OccurredOnUtc =
                        domainEvent.OccurredOnUtc,

                    CreatedOnUtc =
                        DateTime.UtcNow,

                    Status =
                        OutboxMessageStatus.Pending,

                    RetryCount = 0
                };

                context.Set<OutboxMessage>()
                    .Add(outboxMessage);
            }

            entry.Entity.ClearDomainEvents();
        }
    }
}