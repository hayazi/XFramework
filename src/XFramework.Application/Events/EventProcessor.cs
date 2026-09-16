using System.Text.Json;
using XFramework.Application.Contracts.Events;

namespace XFramework.Application.Events;

public sealed class EventProcessor : IEventProcessor
{
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IIdempotencyService _idempotencyService;

    public EventProcessor(
        IEventTypeRegistry eventTypeRegistry,
        IServiceScopeFactory scopeFactory,
        IIdempotencyService idempotencyService)
    {
        _eventTypeRegistry = eventTypeRegistry;
        _scopeFactory = scopeFactory;
        _idempotencyService = idempotencyService;
    }

    public async Task ProcessAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var eventType =
            _eventTypeRegistry.GetEventType(
                envelope.EventType,
                envelope.EventVersion);

        var handlerType =
            typeof(IEventHandler<>)
                .MakeGenericType(eventType);

        var handler =
            scope.ServiceProvider
                .GetRequiredService(handlerType);

        var domainEvent =
            JsonSerializer.Deserialize(
                envelope.Payload,
                eventType)
            ?? throw new NonRetryableEventException(
                "Event payload is invalid.");

        // Invoke handler
    }
}