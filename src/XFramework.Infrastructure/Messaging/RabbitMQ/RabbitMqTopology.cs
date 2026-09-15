using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqTopology
{
    private readonly RabbitMqOptions _options;

    public RabbitMqTopology(RabbitMqOptions options)
    {
        _options = options;
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