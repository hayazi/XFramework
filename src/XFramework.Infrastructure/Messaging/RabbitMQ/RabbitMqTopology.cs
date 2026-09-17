using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqTopology(
    RabbitMqConnectionManager connections,
    IOptions<RabbitMqOptions> options,
    IOptions<RabbitMqRetryOptions> retryOptions)
{
    public async Task InitializeAsync(
        IEnumerable<string> modules,
        CancellationToken cancellationToken = default)
    {
        _ = options.Value;

        var connection = await connections.GetConnectionAsync(cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            RabbitMqNames.MainExchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            RabbitMqNames.RetryExchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            RabbitMqNames.DeadLetterExchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

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
        var queue = RabbitMqNames.Queue(module);

        await channel.QueueDeclareAsync(
            queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue,
            RabbitMqNames.MainExchange,
            RabbitMqNames.MainBindingKey(module),
            arguments: null,
            cancellationToken: cancellationToken);

        var deadLetterQueue = RabbitMqNames.DeadLetterQueue(module);

        await channel.QueueDeclareAsync(
            deadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            deadLetterQueue,
            RabbitMqNames.DeadLetterExchange,
            RabbitMqNames.MainBindingKey(module),
            arguments: null,
            cancellationToken: cancellationToken);

        foreach (var seconds in retryOptions.Value.DelaysInSeconds)
        {
            var delay = RabbitMqRetryDelayNames.FromSeconds(seconds);
            var retryQueue = RabbitMqNames.RetryQueue(module, delay);

            var arguments = new Dictionary<string, object?>
            {
                ["x-message-ttl"] = seconds * 1000,
                ["x-dead-letter-exchange"] = RabbitMqNames.MainExchange,
                ["x-dead-letter-routing-key"] = RabbitMqNames.RetryBindingKey(module, delay)
            };

            await channel.QueueDeclareAsync(
                retryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                retryQueue,
                RabbitMqNames.RetryExchange,
                RabbitMqNames.RetryBindingKey(module, delay),
                arguments: null,
                cancellationToken: cancellationToken);
        }
    }
}
