using System.Collections.Concurrent;
using System.Reflection;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public sealed class EventTypeRegistry : IEventTypeRegistry
{
    private readonly ConcurrentDictionary<
        EventTypeKey,
        Type> _eventTypes = new();

    private readonly ConcurrentDictionary<
        Type,
        EventTypeKey> _reverseLookup = new();

    public Type GetEventType(
        string eventType,
        int eventVersion)
    {
        if (TryGetEventType(
                eventType,
                eventVersion,
                out var clrType))
        {
            return clrType!;
        }

        throw new InvalidOperationException(
            $"Event type '{eventType}' " +
            $"version '{eventVersion}' is not registered.");
    }

    public bool TryGetEventType(
        string eventType,
        int eventVersion,
        out Type? clrType)
    {
        return _eventTypes.TryGetValue(
            new EventTypeKey(
                eventType,
                eventVersion),
            out clrType);
    }

    public string GetEventTypeName(Type clrType)
    {
        if (!_reverseLookup.TryGetValue(
                clrType,
                out var key))
        {
            throw new InvalidOperationException(
                $"Event type '{clrType.FullName}' " +
                "is not registered.");
        }

        return key.Name;
    }

    public int GetEventVersion(Type clrType)
    {
        if (!_reverseLookup.TryGetValue(
                clrType,
                out var key))
        {
            throw new InvalidOperationException(
                $"Event type '{clrType.FullName}' " +
                "is not registered.");
        }

        return key.Version;
    }

    public bool IsRegistered(
        string eventType,
        int eventVersion)
    {
        return _eventTypes.ContainsKey(
            new EventTypeKey(
                eventType,
                eventVersion));
    }

    public void Register(
        Type clrType)
    {
        ArgumentNullException.ThrowIfNull(clrType);

        if (!typeof(IDomainEvent)
            .IsAssignableFrom(clrType))
        {
            throw new InvalidOperationException(
                $"Type '{clrType.FullName}' " +
                "must implement IDomainEvent.");
        }

        var attribute =
            clrType.GetCustomAttribute<EventTypeAttribute>();

        if (attribute is null)
        {
            throw new InvalidOperationException(
                $"Event type '{clrType.FullName}' " +
                "does not have EventTypeAttribute.");
        }

        var key = new EventTypeKey(
            attribute.Name,
            attribute.Version);

        if (!_eventTypes.TryAdd(
                key,
                clrType))
        {
            throw new InvalidOperationException(
                $"Event '{attribute.Name}' " +
                $"version '{attribute.Version}' " +
                "is already registered.");
        }

        if (!_reverseLookup.TryAdd(
                clrType,
                key))
        {
            _eventTypes.TryRemove(key, out _);

            throw new InvalidOperationException(
                $"CLR event type '{clrType.FullName}' " +
                "is already registered.");
        }
    }

    private readonly record struct EventTypeKey(
        string Name,
        int Version);
}