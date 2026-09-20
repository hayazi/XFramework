using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConnectionFactory
{
    private readonly RabbitMqOptions _options;

    public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
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
            RequestedConnectionTimeout = TimeSpan.FromSeconds(
                _options.ConnectionTimeoutSeconds),
            AutomaticRecoveryEnabled = _options.AutomaticRecoveryEnabled,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(
                _options.NetworkRecoveryIntervalSeconds)
        };
    }
}
