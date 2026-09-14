public sealed class EventProcessor : IEventProcessor
{
    private readonly IIdempotencyService _idempotency;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly IUnitOfWork _unitOfWork;

    public EventProcessor(
        IIdempotencyService idempotency,
        IDomainEventDispatcher dispatcher,
        IUnitOfWork unitOfWork)
    {
        _idempotency = idempotency;
        _dispatcher = dispatcher;
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var handlerName =
            GetHandlerName(domainEvent);

        await _unitOfWork.BeginAsync(
            cancellationToken);

        try
        {
            var shouldProcess =
                await _idempotency.TryBeginProcessingAsync(
                    domainEvent.EventId,
                    handlerName,
                    domainEvent.CorrelationId?.ToString(),
                    cancellationToken);

            if (!shouldProcess)
            {
                await _unitOfWork.RollbackAsync(
                    cancellationToken);

                return;
            }

            await _dispatcher.DispatchAsync(
                domainEvent,
                cancellationToken);

            await _unitOfWork.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private static string GetHandlerName(
        IDomainEvent domainEvent)
    {
        return domainEvent.GetType().FullName
            ?? domainEvent.GetType().Name;
    }
}