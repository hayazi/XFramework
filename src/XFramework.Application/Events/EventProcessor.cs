using System.Reflection;
using XFramework.Application.Abstractions;
using XFramework.Application.Contracts.Events;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public sealed class EventProcessor : IEventProcessor
{
    private readonly IEventTypeRegistry _registry;
    private readonly IEventSerializer _serializer;
    private readonly IIdempotencyService _idempotency;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;

    public EventProcessor(
        IEventTypeRegistry registry,
        IEventSerializer serializer,
        IIdempotencyService idempotency,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serializer = serializer;
        _idempotency = idempotency;
        _unitOfWork = unitOfWork;
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        Type eventType;
        try
        {
            eventType = _registry.GetEventType(
                envelope.EventType,
                envelope.EventVersion);
        }
        catch (Exception exception) when (
            exception is InvalidOperationException)
        {
            throw new NonRetryableEventException(
                $"Event '{envelope.EventType}/{envelope.EventVersion}' " +
                "is not registered.",
                exception);
        }

        var domainEvent = _serializer.Deserialize(
            envelope.EventType,
            envelope.EventVersion,
            envelope.Payload);

        var handlerServiceType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handler = _serviceProvider.GetService(handlerServiceType)
            ?? throw new EventHandlerNotRegisteredException(
                $"No handler registered for " +
                $"{envelope.EventType}/{envelope.EventVersion}.");

        var handlerName = handler.GetType().FullName
            ?? handler.GetType().Name;

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var firstProcessing =
                await _idempotency.TryBeginProcessingAsync(
                    envelope.EventId,
                    handlerName,
                    envelope.CorrelationId?.ToString(),
                    cancellationToken);

            if (!firstProcessing)
            {
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            var method = handlerServiceType.GetMethod(
                nameof(IEventHandler<IDomainEvent>.HandleAsync),
                BindingFlags.Instance | BindingFlags.Public);

            if (method is null)
            {
                throw new InvalidOperationException(
                    $"Event handler method was not found for " +
                    $"'{eventType.FullName}'.");
            }

            var task = method.Invoke(
                handler,
                [domainEvent, cancellationToken]) as Task;

            if (task is null)
            {
                throw new InvalidOperationException(
                    "Event handler did not return a Task.");
            }

            await task;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

public sealed class EventHandlerNotRegisteredException
    : NonRetryableEventException
{
    public EventHandlerNotRegisteredException(string message)
        : base(message)
    {
    }
}
