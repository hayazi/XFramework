using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqTopology
{
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly RabbitMqOptions _options;
    private readonly RabbitMqRetryOptions _retryOptions;

    public RabbitMqTopology(
        RabbitMqConnectionManager connectionManager,
        IOptions<RabbitMqOptions> options,
        IOptions<RabbitMqRetryOptions> retryOptions)
    {
        _connectionManager = connectionManager;
        _options = options.Value;
        _retryOptions = retryOptions.Value;
    }

    public async Task InitializeAsync(
        IEnumerable<string> modules,
        CancellationToken cancellationToken = default)
    {
        var connection =
            await _connectionManager.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await DeclareExchangesAsync(
            channel,
            cancellationToken);

        foreach (var module in modules)
        {
            await DeclareModuleTopologyAsync(
                channel,
                module,
                cancellationToken);
        }
    }
    private async Task DeclareExchangesAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqNames.MainExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqNames.RetryExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqNames.DeadLetterExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }
    private async Task DeclareModuleTopologyAsync(
        IChannel channel,
        string module,
        CancellationToken cancellationToken)
    {
        var queue =
            RabbitMqNames.Queue(module);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queue,
            exchange: RabbitMqNames.MainExchange,
            routingKey: RabbitMqNames.MainRoutingKey(module),
            cancellationToken: cancellationToken);

        await DeclareRetryQueuesAsync(
            channel,
            module,
            cancellationToken);

        await DeclareDeadLetterQueueAsync(
            channel,
            module,
            cancellationToken);
    }
    private async Task DeclareRetryQueuesAsync(
        IChannel channel,
        string module,
        CancellationToken cancellationToken)
    {
        foreach (var delaySeconds in
                _retryOptions.DelaysInSeconds)
        {
            var delayName =
                RabbitMqRetryDelayNames
                    .FromSeconds(delaySeconds);

            var queue =
                RabbitMqNames.RetryQueue(
                    module,
                    delayName);

            var routingKey =
                RabbitMqNames.RetryRoutingKey(
                    module,
                    delayName);

            var arguments =
                new Dictionary<string, object?>
                {
                    ["x-message-ttl"] =
                        delaySeconds * 1000,

                    ["x-dead-letter-exchange"] =
                        RabbitMqNames.MainExchange,

                    ["x-dead-letter-routing-key"] =
                        RabbitMqNames.MainRoutingKey(module)
                };

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: queue,
                exchange: RabbitMqNames.RetryExchange,
                routingKey: routingKey,
                cancellationToken: cancellationToken);
        }
    }
    private async Task DeclareDeadLetterQueueAsync(
        IChannel channel,
        string module,
        CancellationToken cancellationToken)
    {
        var queue =
            RabbitMqNames.DeadLetterQueue(module);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queue,
            exchange: RabbitMqNames.DeadLetterExchange,
            routingKey:
                RabbitMqNames.MainBindingKey(module),
            cancellationToken: cancellationToken);
    }
}
