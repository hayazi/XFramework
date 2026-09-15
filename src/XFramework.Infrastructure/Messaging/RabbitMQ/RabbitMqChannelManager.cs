using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqChannelManager
{
    private readonly RabbitMqConnectionManager
        _connectionManager;

    public RabbitMqChannelManager(
        RabbitMqConnectionManager connectionManager)
    {
        _connectionManager =
            connectionManager;
    }

    public async Task<IChannel> CreatePublisherChannelAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await _connectionManager
                .GetConnectionAsync(cancellationToken);

        return await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);
    }

    public async Task<IChannel> CreateConsumerChannelAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await _connectionManager
                .GetConnectionAsync(cancellationToken);

        return await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);
    }
}