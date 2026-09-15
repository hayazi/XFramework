using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqDeadLetterPublisher
    : IEventDeadLetterPublisher
{
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly IEventRoutingResolver _routingResolver;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqDeadLetterPublisher(
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
        string routingKey,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(exception);

        var connection =
            await _connectionManager.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var deadLetterEnvelope = envelope with
        {
            LastError = exception.ToString(),
            LastAttemptOnUtc = DateTime.UtcNow
        };

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(
                deadLetterEnvelope,
                JsonOptions));

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            MessageId = envelope.EventId.ToString(),
            Type = envelope.EventType
        };

        properties.Headers = new Dictionary<string, object?>
        {
            ["event-id"] = envelope.EventId.ToString(),
            ["event-type"] = envelope.EventType,
            ["event-version"] = envelope.EventVersion,
            ["retry-count"] = envelope.RetryCount,
            ["original-routing-key"] = routingKey,
            ["error-type"] = exception.GetType().FullName ?? "Unknown",
            ["error-message"] = exception.Message
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqNames.DeadLetterExchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}