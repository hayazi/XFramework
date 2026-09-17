using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using Microsoft.Extensions.Options;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqEventBus : IEventBus
{
    private readonly RabbitMqChannelManager _channelManager;
    private readonly RabbitMqOptions _options;
    private readonly IEventRoutingResolver _routingResolver;

    public RabbitMqEventBus(
        RabbitMqChannelManager channelManager,
        IOptions<RabbitMqOptions> options,
        IEventRoutingResolver routingResolver)
    {
        _channelManager = channelManager;
        _options = options.Value;
        _routingResolver = routingResolver;
    }

    public async Task PublishAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var routingKey =
            _routingResolver.GetRoutingKey(
                envelope.EventType,
                envelope.EventVersion);

        await using var channel =
            await _channelManager
                .CreatePublisherChannelAsync(
                    cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(
            envelope);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = envelope.EventId.ToString(),
            Type = envelope.EventType,
            Headers = CreateHeaders(envelope)
        };

        await channel.BasicPublishAsync(
            _options.ExchangeName,
            routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private static Dictionary<string, object?> CreateHeaders(
        EventEnvelope envelope)
    {
        return new Dictionary<string, object?>
        {
            ["event-id"] = envelope.EventId.ToString(),
            ["event-type"] = envelope.EventType,
            ["event-version"] = envelope.EventVersion,
            ["retry-count"] = envelope.RetryCount
        };
    }
}