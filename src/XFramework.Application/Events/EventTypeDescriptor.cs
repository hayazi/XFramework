namespace XFramework.Application.Events;

public sealed class EventTypeDescriptor
{
    public EventTypeDescriptor(
        string eventType,
        int version,
        Type clrType)
    {
        EventType = eventType;
        Version = version;
        ClrType = clrType;
    }

    public string EventType { get; }

    public int Version { get; }

    public Type ClrType { get; }
}