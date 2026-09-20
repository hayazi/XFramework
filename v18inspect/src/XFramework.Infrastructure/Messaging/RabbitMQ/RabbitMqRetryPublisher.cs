using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqRetryPublisher : IEventRetryPublisher
{
    private readonly RabbitMqChannelManager _channelManager;
    private readonly RabbitMqOptions _options;

    public RabbitMqRetryPublisher(
        RabbitMqChannelManager channelManager,
        IOptions<RabbitMqOptions> options)
    {
        _channelManager = channelManager;
        _options = options.Value;
    }

    public async Task PublishRetryAsync(
        EventEnvelope envelope,
        string routingKey,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var module = GetModule(routingKey);
        var delayName = RabbitMqRetryDelayNames.FromSeconds(
            checked((int)delay.TotalSeconds));
        var retryRoutingKey = RabbitMqNames.RetryBindingKey(
            module,
            delayName);

        var retryEnvelope = envelope with
        {
            RetryCount = envelope.RetryCount + 1,
            LastAttemptOnUtc = DateTime.UtcNow
        };

        await using var channel =
            await _channelManager.CreatePublisherChannelAsync(
                cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(retryEnvelope);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            MessageId = retryEnvelope.EventId.ToString(),
            Type = retryEnvelope.EventType,
            Headers = new Dictionary<string, object?>
            {
                ["event-id"] = retryEnvelope.EventId.ToString(),
                ["event-type"] = retryEnvelope.EventType,
                ["event-version"] = retryEnvelope.EventVersion,
                ["retry-count"] = retryEnvelope.RetryCount,
                ["original-routing-key"] = routingKey
            }
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqNames.RetryExchange,
            routingKey: retryRoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private static string GetModule(string routingKey)
    {
        var module = routingKey
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(module))
        {
            throw new InvalidOperationException(
                $"Invalid routing key '{routingKey}'.");
        }

        return module;
    }
}
