namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public string ExchangeName { get; set; } = "xframework.events";

    public string ExchangeType { get; set; } = "topic";

    public bool Durable { get; set; } = true;

    public bool AutoDelete { get; set; } = false;

    public bool PublisherConfirms { get; set; } = true;

    public int ConnectionTimeoutSeconds { get; set; } = 10;
}