namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConsumerOptions
{
    public bool Enabled { get; set; } = true;

    public string Module { get; set; } = "accounting";

    public ushort PrefetchCount { get; set; } = 20;

    public string ConsumerName { get; set; } =
        "xframework-consumer";
}