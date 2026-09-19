using RabbitMQ.Client;
using Xunit;

namespace XFramework.IntegrationTests.Infrastructure;

public sealed class RabbitMqIntegrationFixture : IAsyncLifetime
{
    private IConnection? _connection;
    private IChannel? _channel;

    public string ExchangeName => "xframework.integration-tests";
    public string QueueName { get; } = $"xframework.integration-tests.{Guid.NewGuid():N}";

    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            ExchangeName,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        await _channel.QueueDeclareAsync(
            QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);
    }

    public async Task DisposeAsync()
    {
        if (_channel is not null)
        {
            try
            {
                await _channel.QueueDeleteAsync(QueueName);
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException)
            {
                // The test may already have removed the queue.
            }

            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}
