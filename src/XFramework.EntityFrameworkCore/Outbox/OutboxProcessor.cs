using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Application.Outbox;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly IOutboxRetryPolicy _retryPolicy;
    private readonly OutboxOptions _options;

    public OutboxProcessor(
        IOutboxRepository repository,
        IEventBus eventBus,
        IOutboxRetryPolicy retryPolicy,
        Microsoft.Extensions.Options.IOptions<OutboxOptions> options)
    {
        _repository = repository;
        _eventBus = eventBus;
        _retryPolicy = retryPolicy;
        _options = options.Value;
    }

    public async Task ProcessBatchAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await _repository.ReleaseExpiredLeasesAsync(now, cancellationToken);

        var lockId = Guid.NewGuid().ToString("N");
        var messages = await _repository.ClaimBatchAsync(
            batchSize,
            lockId,
            now,
            now.AddMinutes(_options.LeaseMinutes),
            cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var renewNow = DateTime.UtcNow;
                var leaseRenewed = await _repository.RenewLeaseAsync(
                    message.Id,
                    lockId,
                    renewNow,
                    renewNow.AddMinutes(_options.LeaseMinutes),
                    cancellationToken);

                if (!leaseRenewed)
                    continue;

                var envelope = new EventEnvelope
                {
                    EventId = message.EventId,
                    EventType = message.EventType,
                    EventVersion = message.EventVersion,
                    Payload = message.Payload,
                    OccurredOnUtc = message.OccurredOnUtc,
                    CorrelationId = Guid.TryParse(message.CorrelationId, out var correlationId)
                        ? correlationId : null,
                    CausationId = Guid.TryParse(message.CausationId, out var causationId)
                        ? causationId : null,
                    RetryCount = message.RetryCount,
                    LastError = message.LastError
                };

                await _eventBus.PublishAsync(envelope, cancellationToken);

                var completionNow = DateTime.UtcNow;
                await _repository.MarkCompletedAsync(
                    message.Id,
                    lockId,
                    completionNow,
                    completionNow,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                // Application shutdown/cancellation is not a publish failure.
                // Leave the claimed message in Processing so lease recovery can
                // safely make it available again after the worker stops.
                throw;
            }
            catch (Exception exception)
            {
                var error = exception.ToString();

                if (_retryPolicy.ShouldRetry(message.RetryCount, exception))
                {
                    var delay = _retryPolicy.GetDelay(message.RetryCount);
                    var retryNow = DateTime.UtcNow;
                    await _repository.MarkRetryAsync(
                        message.Id,
                        lockId,
                        retryNow,
                        retryNow.Add(delay),
                        error,
                        cancellationToken);
                }
                else
                {
                    await _repository.MarkFailedAsync(
                        message.Id,
                        lockId,
                        DateTime.UtcNow,
                        error,
                        cancellationToken);
                }
            }
        }
    }
}
