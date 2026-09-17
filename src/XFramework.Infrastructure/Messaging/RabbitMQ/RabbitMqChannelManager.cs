using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqChannelManager
{
    private readonly RabbitMqConnectionManager _connectionManager;

    public RabbitMqChannelManager(
        RabbitMqConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default) =>
        _connectionManager.GetConnectionAsync(cancellationToken);

    public async Task<IChannel> CreatePublisherChannelAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);

        var options = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        return await connection.CreateChannelAsync(
            options,
            cancellationToken);
    }

    public async Task<IChannel> CreateConsumerChannelAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);

        return await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);
    }
}
