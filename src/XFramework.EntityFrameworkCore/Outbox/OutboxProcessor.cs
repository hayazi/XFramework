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

        await _repository.ReleaseExpiredLeasesAsync(
            now,
            cancellationToken);

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
                var envelope = new EventEnvelope
                {
                    EventId = message.EventId,
                    EventType = message.EventType,
                    EventVersion = message.EventVersion,
                    Payload = message.Payload,
                    OccurredOnUtc = message.OccurredOnUtc,
                    CorrelationId = Guid.TryParse(
                        message.CorrelationId,
                        out var correlationId)
                        ? correlationId
                        : null,
                    CausationId = Guid.TryParse(
                        message.CausationId,
                        out var causationId)
                        ? causationId
                        : null,
                    RetryCount = message.RetryCount,
                    LastError = message.LastError
                };

                await _eventBus.PublishAsync(
                    envelope,
                    cancellationToken);

                await _repository.MarkCompletedAsync(
                    message.Id,
                    lockId,
                    DateTime.UtcNow,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                if (_retryPolicy.ShouldRetry(
                        message.RetryCount,
                        exception))
                {
                    var delay = _retryPolicy.GetDelay(
                        message.RetryCount);

                    await _repository.MarkFailedAsync(
                        message.Id,
                        lockId,
                        DateTime.UtcNow.Add(delay),
                        exception.ToString(),
                        cancellationToken);
                }
                else
                {
                    await _repository.MarkFailedAsync(
                        message.Id,
                        lockId,
                        DateTime.UtcNow.AddMinutes(5),
                        exception.ToString(),
                        cancellationToken);
                }
            }
        }
    }
}
