using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConsumer : BackgroundService
{
    private readonly RabbitMqMessageHandler _messageHandler;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly RabbitMqChannelManager _channelManager;
     private readonly RabbitMqConsumerOptions _options;
    public RabbitMqConsumer(
        RabbitMqMessageHandler messageHandler,
        RabbitMqChannelManager channelManager;
        RabbitMqConsumerOptions options;
        ILogger<RabbitMqConsumer> logger)
    {
       

        _messageHandler = messageHandler;
        _channelManager = channelManager;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "RabbitMQ consumer starting.");



        await using var channel =
            await _channelManager.CreateConsumerChannelAsync(cancellationToken);


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
                args,
                cancellationToken);
        };

        await channel.BasicConsumeAsync(
                queue,
                autoAck: false,
                consumer);

        await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: _options.PrefetchCount,
                global: false,
                cancellationToken);
    
        await channel.BasicAckAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                cancellationToken);
        _logger.LogInformation(
            "RabbitMQ consumer started.");

        try
        {
            await Task.Delay(
                Timeout.Infinite,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }

        _logger.LogInformation(
            "RabbitMQ consumer stopped.");
    }
}