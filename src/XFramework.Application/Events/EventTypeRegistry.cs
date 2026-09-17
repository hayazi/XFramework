using System.Collections.Concurrent;
using System.Reflection;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public sealed class EventTypeRegistry : IEventTypeRegistry
{
    private readonly ConcurrentDictionary<EventTypeKey,EventTypeRegistration> _byKey=new();
    private readonly ConcurrentDictionary<Type,EventTypeRegistration> _byType=new();
    public void Register(Type clrType)
    {
        ArgumentNullException.ThrowIfNull(clrType);
        if(!typeof(IDomainEvent).IsAssignableFrom(clrType)) throw new InvalidOperationException($"'{clrType.FullName}' must implement IDomainEvent.");
        var a=clrType.GetCustomAttribute<EventTypeAttribute>() ?? throw new InvalidOperationException($"'{clrType.FullName}' has no EventTypeAttribute.");
        var r=new EventTypeRegistration(a.Name,a.Version,clrType,a.RoutingKey);
        if(!_byKey.TryAdd(new(a.Name,a.Version),r)) throw new InvalidOperationException($"Event '{a.Name}/{a.Version}' is already registered.");
        if(!_byType.TryAdd(clrType,r)) { _byKey.TryRemove(new(a.Name,a.Version),out _); throw new InvalidOperationException($"CLR event '{clrType.FullName}' is already registered."); }
    }
    public void RegisterEventsFromAssembly(Assembly assembly){foreach(var t in assembly.GetTypes().Where(t=>t is {IsClass:true,IsAbstract:false} && typeof(IDomainEvent).IsAssignableFrom(t) && t.GetCustomAttribute<EventTypeAttribute>() is not null)) Register(t);}
    public Type GetEventType(string eventType,int eventVersion)=>TryGetEventType(eventType,eventVersion,out var t)&&t is not null?t:throw new InvalidOperationException($"Event '{eventType}/{eventVersion}' is not registered.");
    public bool TryGetEventType(string eventType,int eventVersion,out Type? clrType){var ok=_byKey.TryGetValue(new(eventType,eventVersion),out var r); clrType= r?.ClrType; return ok;}
    public string GetEventTypeName(Type clrType)=>_byType.TryGetValue(clrType,out var r)?r.EventType:throw new InvalidOperationException($"'{clrType.FullName}' is not registered.");
    public int GetEventVersion(Type clrType)=>_byType.TryGetValue(clrType,out var r)?r.Version:throw new InvalidOperationException($"'{clrType.FullName}' is not registered.");
    public string GetRoutingKey(string eventType,int version)=>_byKey.TryGetValue(new(eventType,version),out var r)?r.RoutingKey:throw new InvalidOperationException($"Event '{eventType}/{version}' is not registered.");
    public bool IsRegistered(string eventType,int version)=>_byKey.ContainsKey(new(eventType,version));
    private readonly record struct EventTypeKey(string Name,int Version);
}
public sealed record EventTypeRegistration(string EventType,int Version,Type ClrType,string RoutingKey);
