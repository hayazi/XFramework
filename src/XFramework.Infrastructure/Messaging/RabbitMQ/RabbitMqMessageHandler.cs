using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using XFramework.Application.Events;
using XFramework.Application.Contracts.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqMessageHandler
{
    private readonly IEventProcessor _eventProcessor;
    private readonly IEventRetryPolicy _retryPolicy;
    private readonly IEventRetryPublisher _retryPublisher;
    private readonly IEventDeadLetterPublisher _deadLetterPublisher;
    private readonly ILogger<RabbitMqMessageHandler> _logger;

    public RabbitMqMessageHandler(
        IEventProcessor eventProcessor,
        IEventRetryPolicy retryPolicy,
        IEventRetryPublisher retryPublisher,
        IEventDeadLetterPublisher deadLetterPublisher,
        ILogger<RabbitMqMessageHandler> logger)
    {
        _eventProcessor = eventProcessor;
        _retryPolicy = retryPolicy;
        _retryPublisher = retryPublisher;
        _deadLetterPublisher = deadLetterPublisher;
        _logger = logger;
    }

        private async Task HandleInvalidMessageAsync(
            IChannel channel,
            BasicDeliverEventArgs args,
            byte[] body,
            string? routingKey,
            Exception exception,
            CancellationToken cancellationToken)
        {
            try
            {
                await _deadLetterPublisher.PublishRawAsync(
                    body,
                    routingKey,
                    exception,
                    cancellationToken);

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken);

                _logger.LogError(
                    exception,
                    "Invalid RabbitMQ message moved to raw DLQ.");
            }
            catch (Exception dlqException)
            {
                _logger.LogCritical(
                    dlqException,
                    "Failed to move invalid RabbitMQ message " +
                    "to raw DLQ. Message remains unacknowledged.",
                    dlqException);

                // DO NOT ACK
            }
        }

        private async Task MoveToDeadLetterQueueAsync(
            IChannel channel,
            BasicDeliverEventArgs args,
            EventEnvelope envelope,
            string routingKey,
            Exception exception,
            CancellationToken cancellationToken)
        {
            try
            {
                await _deadLetterPublisher.PublishAsync(
                    envelope,
                    routingKey,
                    exception,
                    cancellationToken);

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken);

                _logger.LogError(
                    exception,
                    "Event {EventId} moved to DLQ. " +
                    "Type={EventType}, Version={EventVersion}",
                    envelope.EventId,
                    envelope.EventType,
                    envelope.EventVersion);
            }
            catch (Exception dlqException)
            {
                _logger.LogCritical(
                    dlqException,
                    "Failed to publish event {EventId} " +
                    "to DLQ. Message remains unacknowledged.",
                    envelope.EventId);

                // DO NOT ACK
            }
        }


        private async Task HandleProcessingFailureAsync(
            IChannel channel,
            BasicDeliverEventArgs args,
            EventEnvelope envelope,
            string routingKey,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var retryCount = envelope.RetryCount;

            if (_retryPolicy.IsRetryable(exception) &&
                _retryPolicy.ShouldRetry(
                    retryCount,
                    exception))
            {
                var delay =
                    _retryPolicy.GetDelay(retryCount);

                try
                {
                    await _retryPublisher.PublishRetryAsync(
                        envelope,
                        routingKey,
                        delay,
                        cancellationToken);

                    await channel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken);

                    _logger.LogWarning(
                        exception,
                        "Event {EventId} scheduled for retry. " +
                        "RetryCount={RetryCount}, Delay={Delay}",
                        envelope.EventId,
                        retryCount,
                        delay);
                }
                catch (Exception retryException)
                {
                    _logger.LogError(
                        retryException,
                        "Failed to publish retry for " +
                        "event {EventId}. Message will remain " +
                        "unacknowledged.",
                        envelope.EventId);

                    // IMPORTANT:
                    // Do NOT ACK.
                }

                return;
            }

            await MoveToDeadLetterQueueAsync(
                channel,
                args,
                envelope,
                routingKey,
                exception,
                cancellationToken);
        }

    public async Task HandleAsync(
        IChannel channel,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        var body = args.Body.ToArray();

        var routingKey = args.RoutingKey;

        EventEnvelope? envelope = null;

        try
        {
            envelope =
                JsonSerializer.Deserialize<EventEnvelope>(body);
        }
        catch (JsonException ex)
        {
            await HandleInvalidMessageAsync(
                channel,
                args,
                body,
                routingKey,
                ex,
                cancellationToken);

            return;
        }

        if (envelope is null)
        {
            await HandleInvalidMessageAsync(
                channel,
                args,
                body,
                routingKey,
                new InvalidOperationException(
                    "Message envelope is null."),
                cancellationToken);

            return;
        }

        try
        {
            await _eventProcessor.ProcessAsync(
                envelope,
                cancellationToken);

            await channel.BasicAckAsync(
                args.DeliveryTag,
                multiple: false,
                cancellationToken);

            _logger.LogInformation(
                "Event {EventId} processed successfully. " +
                "Type={EventType}, Version={EventVersion}",
                envelope.EventId,
                envelope.EventType,
                envelope.EventVersion);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await HandleProcessingFailureAsync(
                channel,
                args,
                envelope,
                routingKey,
                ex,
                cancellationToken);
        }
    }
}