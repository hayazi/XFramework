using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConsumerHostedService : BackgroundService
{
    private readonly RabbitMqConnectionManager _connections;
    private readonly RabbitMqConsumerOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqConsumerHostedService> _logger;

    public RabbitMqConsumerHostedService(
        RabbitMqConnectionManager connections,
        IOptions<RabbitMqConsumerOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqConsumerHostedService> logger)
    {
        _connections = connections;
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("RabbitMQ consumer is disabled.");
            return;
        }

        if (_options.Modules.Length == 0)
        {
            _logger.LogWarning("RabbitMQ consumer has no modules configured.");
            return;
        }

        var connection = await _connections.GetConnectionAsync(
            cancellationToken);

        await using var channel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.PrefetchCount,
            global: false,
            cancellationToken: cancellationToken);

        foreach (var module in _options.Modules
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Select(x => x.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, args) =>
            {
                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var handler = scope.ServiceProvider
                    .GetRequiredService<RabbitMqMessageHandler>();

                await handler.HandleAsync(
                    channel,
                    args,
                    cancellationToken);
            };

            await channel.BasicConsumeAsync(
                queue: RabbitMqNames.Queue(module),
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "RabbitMQ consumer '{ConsumerName}' started for module '{Module}'.",
                _options.ConsumerName,
                module);
        }

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
    }
}
