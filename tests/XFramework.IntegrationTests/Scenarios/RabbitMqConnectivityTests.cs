using Xunit;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using XFramework.IntegrationTests.Infrastructure;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class RabbitMqConnectivityTests : IClassFixture<RabbitMqIntegrationFixture>
{
    private readonly RabbitMqIntegrationFixture _fixture;

    public RabbitMqConnectivityTests(RabbitMqIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RabbitMq_should_publish_and_consume_message()
    {
        var message = $"integration-test-{Guid.NewGuid():N}";
        var received = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueBindAsync(
            _fixture.QueueName,
            _fixture.ExchangeName,
            "integration.test");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            var text = Encoding.UTF8.GetString(args.Body.ToArray());
            received.TrySetResult(text);

            await channel.BasicAckAsync(
                args.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync(
            _fixture.QueueName,
            autoAck: false,
            consumer);

        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            _fixture.ExchangeName,
            "integration.test",
            body);

        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(message, result);
    }
}
