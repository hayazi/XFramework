using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConnectionManager : IAsyncDisposable
{
    private readonly RabbitMqConnectionFactory _factory;

    private IConnection? _connection;

    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqConnectionManager(
        RabbitMqConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = _factory.Create();

            _connection = await factory.CreateConnectionAsync(
                cancellationToken);

            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        _lock.Dispose();
    }
}