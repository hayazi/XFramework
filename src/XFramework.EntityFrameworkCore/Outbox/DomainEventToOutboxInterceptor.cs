using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using XFramework.Application.Events;
using XFramework.Domain.Events;
namespace XFramework.EntityFrameworkCore.Outbox;
public sealed class DomainEventToOutboxInterceptor(IEventTypeRegistry registry):SaveChangesInterceptor
{
 public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,InterceptionResult<int> result,CancellationToken ct=default)
 {
   if(eventData.Context is not DbContext db)return base.SavingChangesAsync(eventData,result,ct);
   var entries=db.ChangeTracker.Entries<IHasDomainEvents>().Where(x=>x.Entity.DomainEvents.Count>0).ToList();
   foreach(var entry in entries){foreach(var e in entry.Entity.DomainEvents){var type=e.GetType();var eventType=registry.GetEventTypeName(type);var version=registry.GetEventVersion(type);var aggregate=entry.Entity;
   var primaryKey = entry.Metadata.FindPrimaryKey();
   string? aggregateId = null;
   if (primaryKey is not null && primaryKey.Properties.Count == 1)
   {
       aggregateId = entry.Property(primaryKey.Properties[0].Name).CurrentValue?.ToString();
   }
   db.Set<OutboxMessage>().Add(new OutboxMessage{Id=Guid.NewGuid(),EventId=e.EventId,EventType=eventType,EventVersion=version,Payload=JsonSerializer.Serialize(e,type),Status=OutboxMessageStatus.Pending,RetryCount=0,CreatedOnUtc=DateTime.UtcNow,OccurredOnUtc=e.OccurredOnUtc,CorrelationId=e.CorrelationId?.ToString(),CausationId=e.CausationId?.ToString(),AggregateType=aggregate.GetType().Name,AggregateId=aggregateId});}entry.Entity.ClearDomainEvents();}
   return base.SavingChangesAsync(eventData,result,ct);
 }
}
