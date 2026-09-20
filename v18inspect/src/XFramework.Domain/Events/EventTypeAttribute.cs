namespace XFramework.Domain.Events;

[AttributeUsage(
    AttributeTargets.Class,
    AllowMultiple = false,
    Inherited = false)]
public sealed class EventTypeAttribute : Attribute
{
    public EventTypeAttribute(
        string name,
        int version,
        string routingKey)
    {
        Name = name;
        Version = version;
        RoutingKey = routingKey;
    }

    public string Name { get; }

    public int Version { get; }

    public string RoutingKey { get; }
}