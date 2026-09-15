using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqRetryPublisher
    : IEventRetryPublisher
{
    private readonly RabbitMqConnectionManager _connectionManager;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqRetryPublisher(
        RabbitMqConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task PublishRetryAsync(
        EventEnvelope envelope,
        string routingKey,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var module = GetModule(routingKey);

        var delayName =
            RabbitMqRetryDelayNames.FromSeconds(
                (int)delay.TotalSeconds);

        var retryQueue =
            RabbitMqNames.RetryQueue(
                module,
                delayName);

        var retryEnvelope = envelope with
        {
            RetryCount = envelope.RetryCount + 1,
            LastAttemptOnUtc = DateTime.UtcNow
        };

        var connection =
            await _connectionManager.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(
                retryEnvelope,
                JsonOptions));

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            MessageId = retryEnvelope.EventId.ToString(),
            Type = retryEnvelope.EventType
        };

        properties.Headers = new Dictionary<string, object?>
        {
            ["event-id"] =
                retryEnvelope.EventId.ToString(),

            ["event-type"] =
                retryEnvelope.EventType,

            ["event-version"] =
                retryEnvelope.EventVersion,

            ["retry-count"] =
                retryEnvelope.RetryCount,

            ["original-routing-key"] =
                routingKey
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqNames.RetryExchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private static string GetModule(
        string routingKey)
    {
        var module =
            routingKey.Split(
                '.',
                StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(module))
        {
            throw new InvalidOperationException(
                $"Invalid routing key '{routingKey}'.");
        }

        return module;
    }
}