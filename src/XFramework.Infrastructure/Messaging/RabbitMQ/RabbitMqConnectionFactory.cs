using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConnectionFactory
{
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly IEventRoutingResolver _routingResolver;

    public RabbitMqConnectionFactory(
        RabbitMqChannelManager channelManager,
        IOptions<RabbitMqOptions> options,
        IEventRoutingResolver routingResolver)
    {
        _channelManager = channelManager;
        _options = options.Value;
        _routingResolver = routingResolver;
    }

    public ConnectionFactory Create()
    {
        return new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,

            UserName = _options.UserName,
            Password = _options.Password,

            VirtualHost = _options.VirtualHost,

            RequestedConnectionTimeout =
                TimeSpan.FromSeconds(
                    _options.ConnectionTimeoutSeconds),

            AutomaticRecoveryEnabled =
                _options.AutomaticRecoveryEnabled,

            NetworkRecoveryInterval =
                TimeSpan.FromSeconds(
                    _options.NetworkRecoveryIntervalSeconds)
        };
    }
}