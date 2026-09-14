using System.Text.Json;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public sealed class EventSerializer : IEventSerializer
{
    private readonly IEventTypeRegistry _registry;
    private readonly JsonSerializerOptions _jsonOptions;

    public EventSerializer(
        IEventTypeRegistry registry,
        JsonSerializerOptions? jsonOptions = null)
    {
        _registry = registry;

        _jsonOptions = jsonOptions ??
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
    }

    public string Serialize(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var descriptor =
            _registry.GetDescriptor(domainEvent.GetType());

        return JsonSerializer.Serialize(
            domainEvent,
            descriptor.ClrType,
            _jsonOptions);
    }

    public IDomainEvent Deserialize(
        string eventType,
        int eventVersion,
        string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        var descriptor =
            _registry.GetDescriptor(
                eventType,
                eventVersion);

        var result =
            JsonSerializer.Deserialize(
                payload,
                descriptor.ClrType,
                _jsonOptions);

        if (result is not IDomainEvent domainEvent)
        {
            throw new InvalidOperationException(
                $"Payload for event '{eventType}' " +
                $"version '{eventVersion}' " +
                $"could not be deserialized.");
        }

        return domainEvent;
    }
}