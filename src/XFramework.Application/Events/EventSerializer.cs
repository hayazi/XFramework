using System.Text.Json;
using XFramework.Domain.Events;
namespace XFramework.Application.Events;
public sealed class EventSerializer(IEventTypeRegistry registry) : IEventSerializer
{
    private static readonly JsonSerializerOptions Options=new(){PropertyNameCaseInsensitive=true};
    public string Serialize(IDomainEvent domainEvent)=>JsonSerializer.Serialize(domainEvent,domainEvent.GetType(),Options);
    public IDomainEvent Deserialize(string eventType,int eventVersion,string payload){var type=registry.GetEventType(eventType,eventVersion); return JsonSerializer.Deserialize(payload,type,Options) as IDomainEvent ?? throw new NonRetryableEventException($"Invalid payload for {eventType}/{eventVersion}.");}
}
