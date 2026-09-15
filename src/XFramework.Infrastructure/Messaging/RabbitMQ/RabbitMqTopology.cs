using RabbitMQ.Client;
using Microsoft.Extensions.Options;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqTopology
{
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly IEventRoutingResolver _routingResolver;

    public RabbitMqTopology(
        RabbitMqChannelManager channelManager,
        IOptions<RabbitMqOptions> options,
        IEventRoutingResolver routingResolver)
    {
        _channelManager = channelManager;
        _options = options.Value;
        _routingResolver = routingResolver;
    }

    public async Task DeclareAsync(
        IChannel channel,
        CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: _options.Durable,
            autoDelete: _options.AutoDelete,
            cancellationToken: cancellationToken);

        await DeclareModuleAsync(
            channel,
            "accounting",
            "accounting.#",
            cancellationToken);

        await DeclareModuleAsync(
            channel,
            "inventory",
            "inventory.#",
            cancellationToken);

        await DeclareModuleAsync(
            channel,
            "sales",
            "sales.#",
            cancellationToken);
    }

    private async Task DeclareModuleAsync(
        IChannel channel,
        string module,
        string routingKey,
        CancellationToken cancellationToken)
    {
        var queue = RabbitMqNames.Queue(module);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queue,
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            cancellationToken: cancellationToken);
    }
}