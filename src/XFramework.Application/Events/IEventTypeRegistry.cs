using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public interface IEventTypeRegistry
{
    Type GetEventType(
        string eventType,
        int eventVersion);

    bool TryGetEventType(
        string eventType,
        int eventVersion,
        out Type? clrType);

    string GetEventTypeName(Type clrType);

    int GetEventVersion(Type clrType);

    bool IsRegistered(
        string eventType,
        int eventVersion);
}