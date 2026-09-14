using Microsoft.Extensions.Logging;
using XFramework.Application.Events;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxProcessor
{
    private readonly IOutboxRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly OutboxRetryPolicy _retryPolicy;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IOutboxRepository repository,
        IEventBus eventBus,
        OutboxRetryPolicy retryPolicy,
        ILogger<OutboxProcessor> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _retryPolicy = retryPolicy;
        _logger = logger;
    }

    public async Task ProcessAsync(
        CancellationToken cancellationToken)
    {
        var claims =
            await _repository.ClaimPendingMessagesAsync(
                batchSize: 50,
                leaseDuration:
                    TimeSpan.FromMinutes(5),
                cancellationToken);

        foreach (var claim in claims)
        {
            try
            {
                await _eventBus.PublishAsync(
                    claim.Message.EventType,
                    claim.Message.Payload,
                    cancellationToken);

                await _repository.MarkAsCompletedAsync(
                    claim.Message,
                    claim.LockId,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await HandleFailureAsync(
                    claim,
                    ex,
                    cancellationToken);
            }
        }
    }

    private async Task HandleFailureAsync(
        OutboxClaim claim,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var message = claim.Message;

        if (!_retryPolicy.CanRetry(
                message.RetryCount))
        {
            await _repository.MarkAsFailedAsync(
                message,
                claim.LockId,
                exception.ToString(),
                DateTime.UtcNow,
                cancellationToken);

            return;
        }

        var delay =
            _retryPolicy.GetDelay(
                message.RetryCount);

        await _repository.MarkAsFailedAsync(
            message,
            claim.LockId,
            exception.ToString(),
            DateTime.UtcNow.Add(delay),
            cancellationToken);

        _logger.LogWarning(
            exception,
            "Outbox message {MessageId} failed.",
            message.Id);
    }
}

