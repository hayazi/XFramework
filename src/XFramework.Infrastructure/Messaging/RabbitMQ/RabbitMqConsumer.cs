using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConsumer : BackgroundService
{
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly RabbitMqTopology _topology;
    private readonly RabbitMqMessageHandler _messageHandler;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(
        RabbitMqConnectionManager connectionManager,
        RabbitMqTopology topology,
        RabbitMqMessageHandler messageHandler,
        ILogger<RabbitMqConsumer> logger)
    {
        _connectionManager = connectionManager;
        _topology = topology;
        _messageHandler = messageHandler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "RabbitMQ consumer starting.");

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
            prefetchCount: _options.PrefetchCount,
            global: false,
            cancellationToken);

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

        await channel.BasicConsumeAsync(
            queue: RabbitMqNames.Queue("accounting"),
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "RabbitMQ consumer started.");

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
            "RabbitMQ consumer stopped.");
    }
}