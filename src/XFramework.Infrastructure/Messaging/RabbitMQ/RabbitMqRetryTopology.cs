using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqRetryTopology
{
    private readonly RabbitMqOptions _options;
    private readonly RabbitMqRetryOptions _retryOptions;
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly IEventRoutingResolver _routingResolver;

    public RabbitMqRetryTopology(
        RabbitMqChannelManager channelManager,
        IOptions<RabbitMqOptions> options,
        IEventRoutingResolver routingResolver
        RabbitMqRetryOptions retryOptions)
    {
        _channelManager = channelManager;
        _options = options.Value;
        _routingResolver = routingResolver;
        _retryOptions = retryOptions;
    }

    public async Task DeclareAsync(
        IChannel channel,
        CancellationToken cancellationToken = default)
    {
        await DeclareMainExchangeAsync(
            channel,
            cancellationToken);

        await DeclareRetryExchangeAsync(
            channel,
            cancellationToken);

        await DeclareDeadLetterExchangeAsync(
            channel,
            cancellationToken);

        await DeclareModulesAsync(
            channel,
            cancellationToken);
    }

    private async Task DeclareMainExchangeAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    private async Task DeclareDeadLetterExchangeAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqNames.DeadLetterExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    private async Task DeclareModulesAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        var modules = new[]
        {
            "accounting",
            "inventory",
            "sales"
        };

        foreach (var module in modules)
        {
            await DeclareModuleAsync(
                channel,
                module,
                cancellationToken);
        }
    }

    private async Task DeclareModuleAsync(
        IChannel channel,
        string module,
        CancellationToken cancellationToken)
    {
        var mainQueue =
            RabbitMqNames.Queue(module);

        var dlq =
            RabbitMqNames.DeadLetterQueue(module);

        // Main queue
        await channel.QueueDeclareAsync(
            queue: mainQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: mainQueue,
            exchange: _options.ExchangeName,
            routingKey: RabbitMqNames.ModuleRoutingKey(module),
            cancellationToken: cancellationToken);

        // DLQ
        await channel.QueueDeclareAsync(
            queue: dlq,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: dlq,
            exchange: RabbitMqNames.DeadLetterExchange,
            routingKey: RabbitMqNames.ModuleRoutingKey(module),
            cancellationToken: cancellationToken);

        await DeclareRetryQueuesAsync(
            channel,
            module,
            cancellationToken);
    }

    private async Task DeclareRetryQueuesAsync(
        IChannel channel,
        string module,
        CancellationToken cancellationToken)
    {
        foreach (var delay in _retryOptions.DelaysInSeconds)
        {
            var delayName =
                RabbitMqRetryDelayNames.FromSeconds(delay);

            var queue =
                RabbitMqNames.RetryQueue(
                    module,
                    delayName);

            var ttl =
                delay * 1000;

            var arguments =
                new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = ttl,

                    ["x-dead-letter-exchange"] =
                        RabbitMqNames.MainExchange
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
                routingKey: RabbitMqNames.ModuleRoutingKey(module),
                cancellationToken: cancellationToken);
        }
    }
    private async Task DeclareRetryExchangeAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqNames.RetryExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }
}