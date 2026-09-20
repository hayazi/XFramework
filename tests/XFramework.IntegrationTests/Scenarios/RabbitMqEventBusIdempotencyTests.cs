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

public sealed class RabbitMqEventBusIdempotencyTests
    : IClassFixture<RabbitMqIntegrationFixture>
{
    private readonly RabbitMqIntegrationFixture _fixture;

    public RabbitMqEventBusIdempotencyTests(
        RabbitMqIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Duplicate_delivery_should_be_processed_only_once()
    {
        var testId = Guid.NewGuid();
        var message = $"idempotency-{Guid.NewGuid():N}";
        var eventId = Guid.NewGuid();
        var routingKey = "integration.test";

        var store = new IntegrationTestStore();
        var idempotency = new TestIdempotencyService();
        var unitOfWork = new TestUnitOfWork();
        var retryPublisher = new TestEventRetryPublisher();
        var deadLetterPublisher = new TestEventDeadLetterPublisher();

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

        var messageHandler = new RabbitMqMessageHandler(
            eventProcessor,
            new DefaultEventRetryPolicy(),
            retryPublisher,
            deadLetterPublisher,
            NullLogger<RabbitMqMessageHandler>.Instance);

        await using var consumerConnection =
            await connectionManager.GetConnectionAsync();
        await using var consumerChannel =
            await consumerConnection.CreateChannelAsync();

        await consumerChannel.QueueBindAsync(
            _fixture.QueueName,
            _fixture.ExchangeName,
            routingKey);

        var deliveryCount = 0;
        var allDeliveriesHandled = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var consumer = new AsyncEventingBasicConsumer(consumerChannel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                await messageHandler.HandleAsync(
                    consumerChannel,
                    args,
                    CancellationToken.None);

                if (Interlocked.Increment(ref deliveryCount) == 2)
                    allDeliveriesHandled.TrySetResult(true);
            }
            catch (Exception exception)
            {
                allDeliveriesHandled.TrySetException(exception);
            }
        };

        await consumerChannel.BasicConsumeAsync(
            queue: _fixture.QueueName,
            autoAck: false,
            consumer: consumer);

        var domainEvent = new IntegrationTestEvent(
            testId,
            message);

        var envelope = new EventEnvelope
        {
            EventId = eventId,
            EventType = "Integration.Test",
            EventVersion = 1,
            Payload = serializer.Serialize(domainEvent),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
            CorrelationId = Guid.NewGuid(),
            CausationId = null,
            RetryCount = 0
        };

        // Publish the exact same event twice. This simulates a duplicate
        // delivery after a broker redelivery/crash window.
        await eventBus.PublishAsync(envelope);
        await eventBus.PublishAsync(envelope);

        await allDeliveriesHandled.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.True(store.Contains(testId));
        Assert.Equal(message, store.Get(testId));

        // The handler must execute only once even though the same EventId
        // was delivered twice.
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
