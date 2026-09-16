using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Events;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public sealed class EventProcessor : IEventProcessor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventTypeRegistry _eventTypeRegistry;

    public EventProcessor(
        IServiceScopeFactory scopeFactory,
        IEventTypeRegistry eventTypeRegistry)
    {
        _scopeFactory = scopeFactory;
        _eventTypeRegistry = eventTypeRegistry;
    }

    public async Task ProcessAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var unitOfWork =
            scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

        var idempotencyService =
            scope.ServiceProvider
                .GetRequiredService<IIdempotencyService>();

        var eventType =
            ResolveEventType(envelope);

        var domainEvent =
            DeserializeEvent(
                envelope,
                eventType);

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await InvokeHandlerAsync(
                scope.ServiceProvider,
                idempotencyService,
                envelope,
                domainEvent,
                eventType,
                cancellationToken);

            await unitOfWork.CommitTransactionAsync(
                cancellationToken);
        }
       //catch
       catch (InvalidOperationException ex)
        {
            await unitOfWork.RollbackTransactionAsync(
                cancellationToken);

            throw;
        }
    }

    private Type ResolveEventType(
        EventEnvelope envelope)
    {
        if (!_eventTypeRegistry.TryGetEventType(
                envelope.EventType,
                envelope.EventVersion,
                out var eventType)
            || eventType is null)
        {
            throw new NonRetryableEventException(
                $"Unknown or unsupported event: " +
                $"{envelope.EventType}/{envelope.EventVersion}");
        }

        return eventType;
    }

    private static object DeserializeEvent(
        EventEnvelope envelope,
        Type eventType)
    {
        try
        {
            return JsonSerializer.Deserialize(
                       envelope.Payload,
                       eventType)
                   ?? throw new NonRetryableEventException(
                       "Event payload is null.");
        }
        catch (JsonException ex)
        {
            throw new NonRetryableEventException(
                $"Invalid payload for event " +
                $"{envelope.EventType}/{envelope.EventVersion}.",
                ex);
        }
    }

    private static async Task InvokeHandlerAsync(
        IServiceProvider serviceProvider,
        IIdempotencyService idempotencyService,
        EventEnvelope envelope,
        object domainEvent,
        Type eventType,
        CancellationToken cancellationToken)
    {
        var handlerInterfaceType =
            typeof(IEventHandler<>)
                .MakeGenericType(eventType);

        var handler =
            serviceProvider
                .GetRequiredService(handlerInterfaceType);

        var handlerName =
            handler.GetType().FullName
            ?? handler.GetType().Name;

        var shouldProcess =
            await idempotencyService
                .TryBeginProcessingAsync(
                    envelope.EventId,
                    handlerName,
                    envelope.CorrelationId?.ToString(),
                    cancellationToken);

        if (!shouldProcess)
        {
            return;
        }

        var method =
            handlerInterfaceType.GetMethod(
                nameof(
                    IEventHandler<IDomainEvent>
                        .HandleAsync));

        if (method is null)
        {
            throw new InvalidOperationException(
                $"HandleAsync was not found " +
                $"for {handlerName}.");
        }

        var result =
            method.Invoke(
                handler,
                new[]
                {
                    domainEvent,
                    cancellationToken
                });

        if (result is not Task task)
        {
            throw new InvalidOperationException(
                $"Handler {handlerName} " +
                $"did not return Task.");
        }

        await task;
    }
}

public sealed class EventHandlerNotRegisteredException
    : NonRetryableEventException
{
    public EventHandlerNotRegisteredException(
        string message)
        : base(message)
    {
    }
}