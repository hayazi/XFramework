using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using Microsoft.Extensions.Options;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqEventBus : IEventBus
{
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly IOptions<RabbitMqOptions> _options;
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

        var connection =
            await _connectionManager.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var routingKey =
            _routingResolver.GetRoutingKey(
                envelope.EventType,
                envelope.EventVersion);

        var body =
            Serialize(envelope);

        var properties =
            CreateProperties(envelope);

        await PublishAndConfirmAsync(
            channel,
            routingKey,
            properties,
            body,
            cancellationToken);
    }
    private async Task PublishAndConfirmAsync(
        IChannel channel,
        string routingKey,
        BasicProperties properties,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken);

        // Broker confirmation is required here.
    }
}