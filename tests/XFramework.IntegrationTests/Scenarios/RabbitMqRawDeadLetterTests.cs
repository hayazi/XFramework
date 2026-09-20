using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;
using XFramework.Application.Events;
using XFramework.Infrastructure.Messaging.RabbitMQ;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class RabbitMqRawDeadLetterTests
{
    private const string Module = "integration-raw-dlq-test";
    private const string RoutingKey = Module + ".event";

    [Fact]
    public async Task Malformed_message_should_be_published_to_raw_dlq_and_original_should_be_acked()
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

        var processor = new UnexpectedProcessor();
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

        var rawBody = "this-is-not-valid-json"u8.ToArray();
        var rawDlqReceived = new TaskCompletionSource<RawDlqMessage>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var dlqConsumer = new AsyncEventingBasicConsumer(channel);
        dlqConsumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var headers = args.BasicProperties.Headers;
                var errorType = GetHeaderString(headers, "error-type");
                var errorMessage = GetHeaderString(headers, "error-message");

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken: CancellationToken.None);

                rawDlqReceived.TrySetResult(
                    new RawDlqMessage(
                        args.Body.ToArray(),
                        args.BasicProperties.ContentType,
                        errorType,
                        errorMessage));
            }
            catch (Exception exception)
            {
                rawDlqReceived.TrySetException(exception);
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

        await channel.BasicPublishAsync(
            exchange: RabbitMqNames.MainExchange,
            routingKey: RoutingKey,
            mandatory: true,
            basicProperties: new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                ContentEncoding = "utf-8"
            },
            body: rawBody);

        await mainProcessed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var deadLetter = await rawDlqReceived.Task.WaitAsync(
            TimeSpan.FromSeconds(5));

        Assert.True(
            processor.CallCount == 0,
            "A malformed message must be rejected before EventProcessor is invoked.");
        Assert.Equal(rawBody, deadLetter.Body);
        Assert.Equal("application/octet-stream", deadLetter.ContentType);
        Assert.Equal(
            typeof(System.Text.Json.JsonException).FullName,
            deadLetter.ErrorType);
        Assert.False(
            string.IsNullOrWhiteSpace(deadLetter.ErrorMessage),
            "Raw DLQ should preserve the deserialization error message.");

        await channel.QueuePurgeAsync(mainQueue);
        await channel.QueuePurgeAsync(dlqQueue);
    }

    private static string? GetHeaderString(
        IDictionary<string, object?>? headers,
        string name)
    {
        if (headers is null || !headers.TryGetValue(name, out var value))
            return null;

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string text => text,
            _ => value?.ToString()
        };
    }

    private sealed record RawDlqMessage(
        byte[] Body,
        string? ContentType,
        string? ErrorType,
        string? ErrorMessage);

    private sealed class UnexpectedProcessor : IEventProcessor
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public Task ProcessAsync(
            XFramework.Application.Contracts.Events.EventEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            throw new InvalidOperationException(
                "EventProcessor must not receive malformed JSON.");
        }
    }
}
