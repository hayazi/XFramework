using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Infrastructure.Messaging.RabbitMQ;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class RabbitMqRetryTests
{
    private const string Module = "integration-retry-test";
    private const string RoutingKey = Module + ".event";

    [Fact]
    public async Task Failed_event_should_be_published_to_retry_queue_and_processed_after_delay()
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
            // Keep the integration test fast while preserving the production retry mechanism.
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

        var queueName = RabbitMqNames.Queue(Module);
        var retryQueueName = RabbitMqNames.RetryQueue(
            Module,
            RabbitMqRetryDelayNames.FromSeconds(5));

        await using var connection =
            await connectionManager.GetConnectionAsync();
        await using var channel =
            await connection.CreateChannelAsync();

        // Remove leftovers from previous failed test runs.
        await channel.QueuePurgeAsync(queueName);
        await channel.QueuePurgeAsync(retryQueueName);

        var processor = new FailOnceEventProcessor();
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

        var completed = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                await messageHandler.HandleAsync(
                    channel,
                    args,
                    CancellationToken.None);

                if (processor.SuccessfulAttemptCount == 1)
                    completed.TrySetResult(true);
            }
            catch (Exception exception)
            {
                completed.TrySetException(exception);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        var eventId = Guid.NewGuid();
        var envelope = new EventEnvelope
        {
            EventId = eventId,
            EventType = "Integration.RetryTest",
            EventVersion = 1,
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid(),
            RetryCount = 0
        };

        // Publish directly to the production-style main exchange/routing topology.
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
                Type = envelope.EventType
            },
            body: System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(envelope));

        // Attempt #1 fails with TimeoutException. The message handler must ACK
        // the original delivery only after successfully publishing retry #1.
        // RabbitMQ then moves retry #1 back to the main exchange after 5 seconds.
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(12));

        Assert.Equal(2, processor.CallCount);
        Assert.Equal(1, processor.FailedAttemptCount);
        Assert.Equal(1, processor.SuccessfulAttemptCount);
        Assert.Equal(0, processor.DlqLikeFailures);
        Assert.Equal(1, processor.ObservedRetryCount);

        await channel.QueuePurgeAsync(queueName);
        await channel.QueuePurgeAsync(retryQueueName);
    }

    private sealed class FailOnceEventProcessor : IEventProcessor
    {
        private int _callCount;
        private int _failedAttemptCount;
        private int _successfulAttemptCount;
        private int _dlqLikeFailures;
        private int _observedRetryCount;

        public int CallCount => Volatile.Read(ref _callCount);
        public int FailedAttemptCount => Volatile.Read(ref _failedAttemptCount);
        public int SuccessfulAttemptCount => Volatile.Read(ref _successfulAttemptCount);
        public int DlqLikeFailures => Volatile.Read(ref _dlqLikeFailures);
        public int ObservedRetryCount => Volatile.Read(ref _observedRetryCount);

        public Task ProcessAsync(
            EventEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);

            if (envelope.RetryCount > 0)
                Interlocked.Exchange(ref _observedRetryCount, envelope.RetryCount);

            if (envelope.RetryCount == 0)
            {
                Interlocked.Increment(ref _failedAttemptCount);
                throw new TimeoutException("Simulated transient failure.");
            }

            Interlocked.Increment(ref _successfulAttemptCount);
            return Task.CompletedTask;
        }
    }
}
