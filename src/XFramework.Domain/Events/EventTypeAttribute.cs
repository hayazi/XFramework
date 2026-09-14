namespace XFramework.Domain.Events;

[AttributeUsage(
    AttributeTargets.Class,
    AllowMultiple = false,
    Inherited = false)]
public sealed class EventTypeAttribute : Attribute
{
    public EventTypeAttribute(string name, int version = 1)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Event type name cannot be empty.",
                nameof(name));

        if (version <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(version));

        Name = name;
        Version = version;
    }

    public string Name { get; }

    public int Version { get; }
}