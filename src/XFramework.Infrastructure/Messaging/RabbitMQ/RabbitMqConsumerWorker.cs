using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConsumerWorker : BackgroundService
{
    private readonly string _module;
    private readonly ushort _prefetchCount;

    private readonly RabbitMqConnectionManager
        _connectionManager;

    private readonly RabbitMqTopology _topology;

    private readonly RabbitMqMessageHandler
        _messageHandler;

    private readonly ILogger<RabbitMqConsumerWorker>
        _logger;

    public RabbitMqConsumerWorker(
        string module,
        ushort prefetchCount,
        RabbitMqConnectionManager connectionManager,
        RabbitMqTopology topology,
        RabbitMqMessageHandler messageHandler,
        ILogger<RabbitMqConsumerWorker> logger)
    {
        _module = module;
        _prefetchCount = prefetchCount;
        _connectionManager = connectionManager;
        _topology = topology;
        _messageHandler = messageHandler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "RabbitMQ consumer for module {Module} starting.",
            _module);

        var connection =
            await _connectionManager.GetConnectionAsync(
                stoppingToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await _topology.DeclareAsync(
            channel,
            stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _prefetchCount,
            global: false,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            await _messageHandler.HandleAsync(
                channel,
                args.DeliveryTag,
                args.Body,
                stoppingToken);
        };

        var queue =
            RabbitMqNames.Queue(_module);

        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "RabbitMQ consumer for {Module} listening on {Queue}.",
            _module,
            queue);

        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }

        _logger.LogInformation(
            "RabbitMQ consumer for {Module} stopped.",
            _module);
    }
}