using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Infrastructure.Messaging.RabbitMQ;
using XFramework.IntegrationTests.Application.Events;
using XFramework.IntegrationTests.Infrastructure;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class RabbitMqCrashWindowTests
    : IClassFixture<RabbitMqIntegrationFixture>
{
    private readonly RabbitMqIntegrationFixture _fixture;

    public RabbitMqCrashWindowTests(RabbitMqIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Commit_before_ack_failure_should_redeliver_and_idempotency_should_prevent_duplicate_effect()
    {
        var testId = Guid.NewGuid();
        var message = $"crash-window-{Guid.NewGuid():N}";
        var eventId = Guid.NewGuid();
        var routingKey = "integration.test";

        var store = new IntegrationTestStore();
        var idempotency = new TestIdempotencyService();
        var unitOfWork = new TestUnitOfWork();

        var registry = new EventTypeRegistry();
        registry.Register(typeof(IntegrationTestEvent));

        var serializer = new EventSerializer(registry);
        var routingResolver = new EventRoutingResolver(registry);

        var eventProcessor = new EventProcessor(
            registry,
            serializer,
            idempotency,
            unitOfWork,
            new TestEventServiceProvider(store));

        var options = Options.Create(new RabbitMqOptions
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            ExchangeName = _fixture.ExchangeName,
            ExchangeType = ExchangeType.Topic,
            PublisherConfirms = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryIntervalSeconds = 5,
            ConnectionTimeoutSeconds = 10
        });

        var connectionFactory = new RabbitMqConnectionFactory(options);
        await using var connectionManager =
            new RabbitMqConnectionManager(connectionFactory);
        var channelManager = new RabbitMqChannelManager(connectionManager);
        var eventBus = new RabbitMqEventBus(
            channelManager,
            options,
            routingResolver);

        var envelope = new EventEnvelope
        {
            EventId = eventId,
            EventType = "Integration.Test",
            EventVersion = 1,
            Payload = serializer.Serialize(
                new IntegrationTestEvent(testId, message)),
            OccurredOnUtc = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid(),
            CausationId = null,
            RetryCount = 0
        };

        // Publish one message.
        await eventBus.PublishAsync(envelope);

        // First delivery: process and COMMIT, but deliberately do not ACK.
        // Closing the channel causes RabbitMQ to requeue the unacked message,
        // which models a process/channel crash after the transaction commit
        // but before the broker acknowledgement.
        await using var firstConnection =
            await connectionManager.GetConnectionAsync();
        await using var firstChannel =
            await firstConnection.CreateChannelAsync();

        await firstChannel.QueueBindAsync(
            _fixture.QueueName,
            _fixture.ExchangeName,
            routingKey);

        var firstDeliveryProcessed =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var firstConsumer = new AsyncEventingBasicConsumer(firstChannel);
        firstConsumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                Assert.False(
                    args.Redelivered,
                    "The first delivery must not be a broker redelivery.");

                await eventProcessor.ProcessAsync(
                    envelope,
                    CancellationToken.None);

                firstDeliveryProcessed.TrySetResult(true);

                // No BasicAckAsync here: the message remains unacknowledged.
                await firstChannel.CloseAsync();
            }
            catch (Exception exception)
            {
                firstDeliveryProcessed.TrySetException(exception);
            }
        };

        await firstChannel.BasicConsumeAsync(
            queue: _fixture.QueueName,
            autoAck: false,
            consumer: firstConsumer);

        await firstDeliveryProcessed.Task.WaitAsync(
            TimeSpan.FromSeconds(10));

        Assert.True(store.Contains(testId));
        Assert.Equal(message, store.Get(testId));
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, unitOfWork.CommitCallCount);

        // Second delivery: RabbitMQ redelivers the unacked message. The normal
        // message handler must ACK it, but EventProcessor must detect the
        // already-committed EventId and avoid the business side effect again.
        var retryPublisher = new TestEventRetryPublisher();
        var deadLetterPublisher = new TestEventDeadLetterPublisher();
        var messageHandler = new RabbitMqMessageHandler(
            eventProcessor,
            new DefaultEventRetryPolicy(),
            retryPublisher,
            deadLetterPublisher,
            NullLogger<RabbitMqMessageHandler>.Instance);

        await using var secondConnection =
            await connectionManager.GetConnectionAsync();
        await using var secondChannel =
            await secondConnection.CreateChannelAsync();

        await secondChannel.QueueBindAsync(
            _fixture.QueueName,
            _fixture.ExchangeName,
            routingKey);

        var redeliveryHandled =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var secondConsumer = new AsyncEventingBasicConsumer(secondChannel);
        secondConsumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                Assert.True(
                    args.Redelivered,
                    "The second delivery must be marked as a RabbitMQ redelivery.");

                await messageHandler.HandleAsync(
                    secondChannel,
                    args,
                    CancellationToken.None);

                redeliveryHandled.TrySetResult(true);
            }
            catch (Exception exception)
            {
                redeliveryHandled.TrySetException(exception);
            }
        };

        await secondChannel.BasicConsumeAsync(
            queue: _fixture.QueueName,
            autoAck: false,
            consumer: secondConsumer);

        await redeliveryHandled.Task.WaitAsync(
            TimeSpan.FromSeconds(10));

        Assert.True(store.Contains(testId));
        Assert.Equal(message, store.Get(testId));

        // The committed business effect happened exactly once.
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, unitOfWork.CommitCallCount);

        Assert.Equal(0, retryPublisher.CallCount);
        Assert.Equal(0, deadLetterPublisher.CallCount);
    }

    private sealed class TestEventServiceProvider(
        IntegrationTestStore store) : IServiceProvider
    {
        private readonly IntegrationTestEventHandler _handler =
            new(store);

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IEventHandler<IntegrationTestEvent>))
                return _handler;

            return null;
        }
    }
}
