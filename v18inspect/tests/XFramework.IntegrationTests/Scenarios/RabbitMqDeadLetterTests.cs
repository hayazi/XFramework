using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Infrastructure.Messaging.RabbitMQ;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class RabbitMqDeadLetterTests
{
    private const string Module = "integration-dlq-test";
    private const string RoutingKey = Module + ".event";

    [Fact]
    public async Task Non_retryable_event_should_be_published_to_dlq_and_original_should_be_acked()
    {
        var options = Options.Create(new RabbitMqOptions
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            ExchangeName = RabbitMqNames.MainExchange,
            ExchangeType = ExchangeType.Topic,
            PublisherConfirms = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryIntervalSeconds = 5,
            ConnectionTimeoutSeconds = 10
        });

        var retryOptions = Options.Create(new RabbitMqRetryOptions
        {
            DelaysInSeconds = [5, 30, 120, 600, 1800]
        });

        var connectionFactory = new RabbitMqConnectionFactory(options);
        await using var connectionManager =
            new RabbitMqConnectionManager(connectionFactory);

        var topology = new RabbitMqTopology(
            connectionManager,
            options,
            retryOptions);

        await topology.InitializeAsync([Module]);

        await using var connection =
            await connectionManager.GetConnectionAsync();
        await using var channel =
            await connection.CreateChannelAsync();

        var mainQueue = RabbitMqNames.Queue(Module);
        var dlqQueue = RabbitMqNames.DeadLetterQueue(Module);

        await channel.QueuePurgeAsync(mainQueue);
        await channel.QueuePurgeAsync(dlqQueue);

        var processor = new NonRetryableEventProcessor();
        var retryPublisher = new RabbitMqRetryPublisher(
            new RabbitMqChannelManager(connectionManager),
            options);
        var deadLetterPublisher = new RabbitMqDeadLetterPublisher(
            new RabbitMqChannelManager(connectionManager),
            options);

        var messageHandler = new RabbitMqMessageHandler(
            processor,
            new DefaultEventRetryPolicy(),
            retryPublisher,
            deadLetterPublisher,
            NullLogger<RabbitMqMessageHandler>.Instance);

        var dlqReceived = new TaskCompletionSource<EventEnvelope>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var dlqConsumer = new AsyncEventingBasicConsumer(channel);
        dlqConsumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<EventEnvelope>(
                    args.Body.Span,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                Assert.NotNull(envelope);

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken: CancellationToken.None);

                dlqReceived.TrySetResult(envelope!);
            }
            catch (Exception exception)
            {
                dlqReceived.TrySetException(exception);
            }
        };

        await channel.BasicConsumeAsync(
            queue: dlqQueue,
            autoAck: false,
            consumer: dlqConsumer);

        var mainProcessed = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var mainConsumer = new AsyncEventingBasicConsumer(channel);
        mainConsumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                await messageHandler.HandleAsync(
                    channel,
                    args,
                    CancellationToken.None);

                mainProcessed.TrySetResult(true);
            }
            catch (Exception exception)
            {
                mainProcessed.TrySetException(exception);
            }
        };

        await channel.BasicConsumeAsync(
            queue: mainQueue,
            autoAck: false,
            consumer: mainConsumer);

        var eventId = Guid.NewGuid();
        var envelopeToPublish = new EventEnvelope
        {
            EventId = eventId,
            EventType = "Integration.DeadLetterTest",
            EventVersion = 1,
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid(),
            RetryCount = 0
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqNames.MainExchange,
            routingKey: RoutingKey,
            mandatory: true,
            basicProperties: new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                MessageId = eventId.ToString(),
                Type = envelopeToPublish.EventType
            },
            body: JsonSerializer.SerializeToUtf8Bytes(envelopeToPublish));

        await mainProcessed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var deadLetterEnvelope = await dlqReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(1, processor.CallCount);
        Assert.Equal(0, processor.RetryLikeFailures);
        Assert.Equal(eventId, deadLetterEnvelope.EventId);
        Assert.Equal(envelopeToPublish.EventType, deadLetterEnvelope.EventType);
        Assert.Equal(0, deadLetterEnvelope.RetryCount);
        Assert.NotNull(deadLetterEnvelope.LastError);
        Assert.Contains("Non-retryable test failure", deadLetterEnvelope.LastError!);

        await channel.QueuePurgeAsync(mainQueue);
        await channel.QueuePurgeAsync(dlqQueue);
    }

    private sealed class NonRetryableEventProcessor : IEventProcessor
    {
        private int _callCount;
        private int _retryLikeFailures;
        public int CallCount => Volatile.Read(ref _callCount);
        public int RetryLikeFailures => Volatile.Read(ref _retryLikeFailures);

        public Task ProcessAsync(
            EventEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);

            if (envelope.RetryCount > 0)
                Interlocked.Increment(ref _retryLikeFailures);

            throw new InvalidOperationException("Non-retryable test failure.");
        }
    }
}
