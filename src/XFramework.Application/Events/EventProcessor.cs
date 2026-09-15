using System.Text.Json;
using XFramework.Application.Contracts.Events;

namespace XFramework.Application.Events;

public sealed class EventProcessor : IEventProcessor
{
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly IServiceProvider _serviceProvider;

    public EventProcessor(
        IEventTypeRegistry eventTypeRegistry,
        IServiceProvider serviceProvider)
    {
        _eventTypeRegistry = eventTypeRegistry;
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var eventType = _eventTypeRegistry.GetEventType(
            envelope.EventType,
            envelope.EventVersion);

        var domainEvent =
            JsonSerializer.Deserialize(
                envelope.Payload,
                eventType);

        if (domainEvent is null)
        {
            throw new InvalidOperationException(
                $"Unable to deserialize event '{envelope.EventType}'.");
        }

        var handlerType =
            typeof(IDomainEventHandler<>)
                .MakeGenericType(eventType);

        var handler =
            _serviceProvider.GetService(handlerType);

        if (handler is null)
        {
            throw new InvalidOperationException(
                $"No handler registered for event '{envelope.EventType}'.");
        }

        var method =
            handlerType.GetMethod(nameof(
                IDomainEventHandler<IDomainEvent>.HandleAsync));

        if (method is null)
        {
            throw new InvalidOperationException(
                $"Handler method was not found for '{envelope.EventType}'.");
        }

        var task = (Task?)method.Invoke(
            handler,
            new object?[]
            {
                domainEvent,
                cancellationToken
            });

        if (task is not null)
            await task;
    }
}