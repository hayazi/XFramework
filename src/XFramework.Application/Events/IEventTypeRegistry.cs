using System.Reflection;
namespace XFramework.Application.Events;
public interface IEventTypeRegistry
{
    Type GetEventType(string eventType,int eventVersion);
    bool TryGetEventType(string eventType,int eventVersion,out Type? clrType);
    string GetEventTypeName(Type clrType);
    int GetEventVersion(Type clrType);
    string GetRoutingKey(string eventType,int eventVersion);
    bool IsRegistered(string eventType,int eventVersion);
    void Register(Type clrType);
    void RegisterEventsFromAssembly(Assembly assembly);
}
