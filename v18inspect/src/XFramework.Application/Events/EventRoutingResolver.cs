namespace XFramework.Application.Events;
public sealed class EventRoutingResolver(IEventTypeRegistry registry) : IEventRoutingResolver
{ public string GetRoutingKey(string eventType,int eventVersion)=>registry.GetRoutingKey(eventType,eventVersion); }
