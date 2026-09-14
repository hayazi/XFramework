namespace XFramework.Application.Events;

public interface IEventTypeRegistry
{
    void Register<TEvent>();

    EventTypeDescriptor GetDescriptor(Type eventType);

    EventTypeDescriptor GetDescriptor(
        string eventType,
        int version);

    bool TryGetDescriptor(
        string eventType,
        int version,
        out EventTypeDescriptor? descriptor);
}