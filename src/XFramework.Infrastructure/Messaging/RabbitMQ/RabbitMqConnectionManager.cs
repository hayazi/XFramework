using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConnectionManager
    : IAsyncDisposable
{
    private readonly RabbitMqConnectionFactory _factory;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private IConnection? _connection;

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

            await DisposeConnectionAsync();

            var factory = _factory.Create();

            _connection =
                await factory.CreateConnectionAsync(
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
        await _lock.WaitAsync();

        try
        {
            await DisposeConnectionAsync();
        }
        finally
        {
            _lock.Release();
            _lock.Dispose();
        }
    }

    private async Task DisposeConnectionAsync()
    {
        if (_connection is null)
            return;

        try
        {
            await _connection.DisposeAsync();
        }
        catch
        {
            // Logging will be added in the
            // observability stage.
        }
        finally
        {
            _connection = null;
        }
    }
}