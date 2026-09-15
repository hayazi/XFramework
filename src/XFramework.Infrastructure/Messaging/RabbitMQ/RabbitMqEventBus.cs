using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqEventBus : IEventBus
{
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly RabbitMqOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqEventBus(
        RabbitMqConnectionManager connectionManager,
        RabbitMqOptions options)
    {
        _connectionManager = connectionManager;
        _options = options;
    }

    public async Task PublishAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var connection =
            await _connectionManager.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(
                envelope,
                JsonOptions));

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            MessageId = envelope.EventId.ToString(),
            Type = envelope.EventType,
            Headers = CreateHeaders(envelope)
        };

        var routingKey =
            ToRoutingKey(envelope.EventType);

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
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
            ["occurred-on-utc"] =
                envelope.OccurredOnUtc.ToString("O"),
            ["correlation-id"] =
                envelope.CorrelationId?.ToString(),
            ["causation-id"] =
                envelope.CausationId?.ToString()
        };
    }

    private static string ToRoutingKey(
        string eventType)
    {
        return eventType
            .Replace('.', '.')
            .ToLowerInvariant();
    }
}