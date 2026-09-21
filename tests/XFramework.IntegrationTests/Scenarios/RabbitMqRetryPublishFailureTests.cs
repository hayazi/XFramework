using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Infrastructure.Messaging.RabbitMQ;
using XFramework.IntegrationTests.Infrastructure;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class RabbitMqRetryPublishFailureTests
    : IClassFixture<RabbitMqIntegrationFixture>
{
    private readonly RabbitMqIntegrationFixture _fixture;

    public RabbitMqRetryPublishFailureTests(RabbitMqIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Retry_publish_failure_should_leave_original_unacked_and_broker_should_redeliver()
    {
        var routingKey = "integration.test.retry-failure";
        var eventId = Guid.NewGuid();
        var envelope = new EventEnvelope
        {
            EventId = eventId,
            EventType = "Integration.Test",
            EventVersion = 1,
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        var processor = new FailOnceThenSucceedProcessor();
        var retryPublisher = new ThrowingRetryPublisher();
        var deadLetterPublisher = new TestDeadLetterPublisher();
        var handler = new RabbitMqMessageHandler(
            processor,
            new DefaultEventRetryPolicy(),
            retryPublisher,
            deadLetterPublisher,
            NullLogger<RabbitMqMessageHandler>.Instance);

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };

        await using var firstConnection = await factory.CreateConnectionAsync();
        await using var firstChannel = await firstConnection.CreateChannelAsync();

        await firstChannel.QueueBindAsync(
            _fixture.QueueName,
            _fixture.ExchangeName,
            routingKey);

        var body = JsonSerializer.SerializeToUtf8Bytes(envelope);

        await firstChannel.BasicPublishAsync(
            _fixture.ExchangeName,
            routingKey,
            mandatory: true,
            basicProperties: new BasicProperties(),
            body: body,
            cancellationToken: CancellationToken.None);

        var firstAttempt = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var firstConsumer = new AsyncEventingBasicConsumer(firstChannel);
        firstConsumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                Assert.False(args.Redelivered);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    handler.HandleAsync(
                        firstChannel,
                        args,
                        CancellationToken.None));

                firstAttempt.TrySetResult(true);
            }
            catch (Exception exception)
            {
                firstAttempt.TrySetException(exception);
            }
        };

        await firstChannel.BasicConsumeAsync(
            _fixture.QueueName,
            autoAck: false,
            consumer: firstConsumer);

        await firstAttempt.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(1, processor.CallCount);
        Assert.Equal(1, retryPublisher.CallCount);
        Assert.Equal(0, deadLetterPublisher.CallCount);

        // The retry publisher failed, so RabbitMqMessageHandler must not ACK
        // the original delivery. Closing the connection simulates a consumer
        // crash and RabbitMQ must requeue the unacknowledged message.
        await firstConnection.CloseAsync();

        await using var secondConnection = await factory.CreateConnectionAsync();
        await using var secondChannel = await secondConnection.CreateChannelAsync();

        await secondChannel.QueueBindAsync(
            _fixture.QueueName,
            _fixture.ExchangeName,
            routingKey);

        var redeliveryHandled = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var secondConsumer = new AsyncEventingBasicConsumer(secondChannel);
        secondConsumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                Assert.True(
                    args.Redelivered,
                    "The message must be redelivered because the retry publish failed before ACK.");

                await handler.HandleAsync(
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
            _fixture.QueueName,
            autoAck: false,
            consumer: secondConsumer);

        await redeliveryHandled.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(2, processor.CallCount);
        Assert.Equal(1, retryPublisher.CallCount);
        Assert.Equal(0, deadLetterPublisher.CallCount);
    }

    private sealed class FailOnceThenSucceedProcessor : IEventProcessor
    {
        public int CallCount { get; private set; }

        public Task ProcessAsync(
            EventEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            if (CallCount == 1)
                throw new TimeoutException("Simulated transient processing failure.");

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingRetryPublisher : IEventRetryPublisher
    {
        public int CallCount { get; private set; }

        public Task PublishRetryAsync(
            EventEnvelope envelope,
            string routingKey,
            TimeSpan delay,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Simulated retry publish failure.");
        }
    }

    private sealed class TestDeadLetterPublisher : IEventDeadLetterPublisher
    {
        public int CallCount { get; private set; }

        public Task PublishAsync(
            EventEnvelope envelope,
            string routingKey,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }

        public Task PublishRawAsync(
            byte[] body,
            string? routingKey,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }
}
