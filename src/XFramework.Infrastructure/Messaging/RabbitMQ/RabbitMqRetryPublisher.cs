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
    private readonly RabbitMqOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqRetryPublisher(
        RabbitMqConnectionManager connectionManager,
        RabbitMqOptions options)
    {
        _connectionManager = connectionManager;
        _options = options;
    }

    public async Task PublishRetryAsync(
        EventEnvelope envelope,
        string routingKey,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var connection =
            await _connectionManager.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var delayName =
            RabbitMqRetryDelayNames.FromSeconds(
                (int)delay.TotalSeconds);

        var module =
            GetModuleFromRoutingKey(routingKey);

        var retryQueue =
            RabbitMqNames.RetryQueue(
                module,
                delayName);

        var body =
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(
                    envelope,
                    JsonOptions));

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            MessageId = envelope.EventId.ToString(),
            Type = envelope.EventType
        };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: GetRetryRoutingKey(
                retryQueue),
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private static string GetModuleFromRoutingKey(
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

    private static string GetRetryRoutingKey(
        string retryQueue)
    {
        return retryQueue;
    }
}