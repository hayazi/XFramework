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

public sealed class RabbitMqDeadLetterPublishFailureTests
    : IClassFixture<RabbitMqIntegrationFixture>
{
    private readonly RabbitMqIntegrationFixture _fixture;

    public RabbitMqDeadLetterPublishFailureTests(RabbitMqIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dead_letter_publish_failure_should_leave_original_unacked_and_broker_should_redeliver()
    {
        var routingKey = "integration.test.dlq-failure";
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

        var processor = new AlwaysFailNonRetryableProcessor();
        var retryPublisher = new TestEventRetryPublisher();
        var failingDeadLetterPublisher = new ThrowingDeadLetterPublisher();
        var firstHandler = new RabbitMqMessageHandler(
            processor,
            new DefaultEventRetryPolicy(),
            retryPublisher,
            failingDeadLetterPublisher,
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
                    firstHandler.HandleAsync(
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
        Assert.Equal(0, retryPublisher.CallCount);
        Assert.Equal(1, failingDeadLetterPublisher.CallCount);

        // DLQ publication failed, therefore the original delivery must remain
        // unacknowledged. Closing the connection simulates the consumer crash;
        // RabbitMQ must requeue the original message.
        await firstConnection.CloseAsync();

        var successfulDeadLetterPublisher = new TestEventDeadLetterPublisher();
        var secondHandler = new RabbitMqMessageHandler(
            processor,
            new DefaultEventRetryPolicy(),
            retryPublisher,
            successfulDeadLetterPublisher,
            NullLogger<RabbitMqMessageHandler>.Instance);

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
                    "The message must be redelivered because DLQ publication failed before ACK.");

                // On redelivery the DLQ publisher is healthy, so the handler
                // must successfully publish to the DLQ and then ACK the
                // original message. No exception is expected here.
                await secondHandler.HandleAsync(
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
        Assert.Equal(0, retryPublisher.CallCount);
        Assert.Equal(1, successfulDeadLetterPublisher.CallCount);
    }

    private sealed class AlwaysFailNonRetryableProcessor : IEventProcessor
    {
        public int CallCount { get; private set; }

        public Task ProcessAsync(
            EventEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Simulated non-retryable processing failure.");
        }
    }

    private sealed class ThrowingDeadLetterPublisher : IEventDeadLetterPublisher
    {
        public int CallCount { get; private set; }

        public Task PublishAsync(
            EventEnvelope envelope,
            string routingKey,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Simulated DLQ publish failure.");
        }

        public Task PublishRawAsync(
            byte[] body,
            string? routingKey,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Simulated raw DLQ publish failure.");
        }
    }
}
