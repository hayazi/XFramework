using System.Collections.Concurrent;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public sealed class EventTypeRegistry : IEventTypeRegistry
{
    private readonly ConcurrentDictionary<Type, EventTypeDescriptor>
        _byClrType = new();

    private readonly ConcurrentDictionary<string, EventTypeDescriptor>
        _byContract = new();

    public void Register<TEvent>()
    {
        Register(typeof(TEvent));
    }

    private void Register(Type clrType)
    {
        if (!typeof(IDomainEvent).IsAssignableFrom(clrType))
        {
            throw new InvalidOperationException(
                $"Type '{clrType.FullName}' is not a domain event.");
        }

        var attribute = clrType
            .GetCustomAttributes(typeof(EventTypeAttribute), false)
            .Cast<EventTypeAttribute>()
            .SingleOrDefault();

        if (attribute is null)
        {
            throw new InvalidOperationException(
                $"Event type '{clrType.FullName}' does not define " +
                $"{nameof(EventTypeAttribute)}.");
        }

        var descriptor = new EventTypeDescriptor(
            attribute.Name,
            attribute.Version,
            clrType);

        if (!_byClrType.TryAdd(clrType, descriptor))
        {
            throw new InvalidOperationException(
                $"Event CLR type '{clrType.FullName}' is already registered.");
        }

        var key = CreateKey(
            attribute.Name,
            attribute.Version);

        if (!_byContract.TryAdd(key, descriptor))
        {
            _byClrType.TryRemove(clrType, out _);

            throw new InvalidOperationException(
                $"Event contract '{attribute.Name}' " +
                $"version '{attribute.Version}' is already registered.");
        }
    }

    public EventTypeDescriptor GetDescriptor(Type eventType)
    {
        if (_byClrType.TryGetValue(eventType, out var descriptor))
            return descriptor;

        throw new InvalidOperationException(
            $"Event type '{eventType.FullName}' is not registered.");
    }

    public EventTypeDescriptor GetDescriptor(
        string eventType,
        int version)
    {
        if (TryGetDescriptor(eventType, version, out var descriptor))
            return descriptor!;

        throw new InvalidOperationException(
            $"Event contract '{eventType}' version '{version}' " +
            "is not registered.");
    }

    public bool TryGetDescriptor(
        string eventType,
        int version,
        out EventTypeDescriptor? descriptor)
    {
        return _byContract.TryGetValue(
            CreateKey(eventType, version),
            out descriptor);
    }

    private static string CreateKey(
        string eventType,
        int version)
    {
        return $"{eventType}:{version}";
    }
}