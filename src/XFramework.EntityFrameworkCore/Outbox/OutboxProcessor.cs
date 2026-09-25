using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IOutboxRepository repository,
        IEventBus eventBus,
        IOutboxRetryPolicy retryPolicy,
        Microsoft.Extensions.Options.IOptions<OutboxOptions> options,
        ILogger<OutboxProcessor>? logger = null)
    {
        _repository = repository;
        _eventBus = eventBus;
        _retryPolicy = retryPolicy;
        _options = options.Value;
        _logger = logger ?? NullLogger<OutboxProcessor>.Instance;
    }

    public async Task ProcessBatchAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var released = await _repository.ReleaseExpiredLeasesAsync(now, cancellationToken);
        if (released > 0)
        {
            OutboxDiagnostics.LeaseExpiredRecovered.Add(released);
        }

        var lockId = Guid.NewGuid().ToString("N");
        var messages = await _repository.ClaimBatchAsync(
            batchSize,
            lockId,
            now,
            now.AddMinutes(_options.LeaseMinutes),
            cancellationToken);

        foreach (var message in messages)
        {
            var messageStart = Stopwatch.GetTimestamp();
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
                {
                    OutboxDiagnostics.LeaseRenewalLost.Add(1);
                    _logger.LogWarning(
                        "Outbox lease renewal was lost for message {MessageId} with lock {LockId}.",
                        message.Id,
                        lockId);
                    continue;
                }

                using var activity = OutboxDiagnostics.ActivitySource.StartActivity(
                    "outbox.publish",
                    ActivityKind.Producer);

                activity?.SetTag("messaging.system", "rabbitmq");
                activity?.SetTag("messaging.destination", message.EventType);
                activity?.SetTag("messaging.message_id", message.EventId.ToString());
                if (!string.IsNullOrWhiteSpace(message.CorrelationId))
                {
                    activity?.SetTag("messaging.correlation_id", message.CorrelationId);
                }
                if (!string.IsNullOrWhiteSpace(message.CausationId))
                {
                    activity?.SetTag("messaging.causation_id", message.CausationId);
                }
                activity?.SetTag("messaging.message_retry_count", message.RetryCount);

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
                    LastError = message.LastError,
                    TraceParent = activity?.TraceId.ToString(),
                    TraceState = activity?.TraceStateString
                };

                await _eventBus.PublishAsync(envelope, cancellationToken);
                OutboxDiagnostics.Published.Add(1);

                var completionNow = DateTime.UtcNow;
                var completed = await _repository.MarkCompletedAsync(
                    message.Id,
                    lockId,
                    completionNow,
                    completionNow,
                    cancellationToken);

                if (!completed)
                {
                    OutboxDiagnostics.CompletionOwnershipLost.Add(1);
                    _logger.LogWarning(
                        "Outbox completion ownership was lost after publish for message {MessageId}, event {EventId}, lock {LockId}. The event may be published again by a later worker.",
                        message.Id,
                        message.EventId,
                        lockId);
                    continue;
                }

                OutboxDiagnostics.Completed.Add(1);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var error = exception.ToString();

                if (_retryPolicy.ShouldRetry(message.RetryCount, exception))
                {
                    var delay = _retryPolicy.GetDelay(message.RetryCount);
                    var retryNow = DateTime.UtcNow;
                    var retried = await _repository.MarkRetryAsync(
                        message.Id,
                        lockId,
                        retryNow,
                        retryNow.Add(delay),
                        error,
                        cancellationToken);

                    if (retried)
                    {
                        OutboxDiagnostics.Retried.Add(1);
                    }
                    else
                    {
                        OutboxDiagnostics.RetryOwnershipLost.Add(1);
                        _logger.LogWarning(
                            exception,
                            "Outbox retry ownership was lost for message {MessageId}, event {EventId}, lock {LockId}.",
                            message.Id,
                            message.EventId,
                            lockId);
                    }
                }
                else
                {
                    var failed = await _repository.MarkFailedAsync(
                        message.Id,
                        lockId,
                        DateTime.UtcNow,
                        error,
                        cancellationToken);

                    if (failed)
                    {
                        OutboxDiagnostics.Failed.Add(1);
                    }
                    else
                    {
                        OutboxDiagnostics.FailureOwnershipLost.Add(1);
                        _logger.LogWarning(
                            exception,
                            "Outbox failure ownership was lost for message {MessageId}, event {EventId}, lock {LockId}.",
                            message.Id,
                            message.EventId,
                            lockId);
                    }
                }
            }
            finally
            {
                var elapsed = Stopwatch.GetElapsedTime(messageStart);
                OutboxDiagnostics.ProcessDuration.Record(elapsed.TotalSeconds);
            }
        }
    }
}
