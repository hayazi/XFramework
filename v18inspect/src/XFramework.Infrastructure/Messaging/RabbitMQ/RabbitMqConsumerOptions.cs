namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqConsumerOptions
{
    public const string SectionName = "RabbitMQ:Consumer";

    public bool Enabled { get; set; } = true;

    public string[] Modules { get; set; } =
    [
        "accounting",
        "inventory",
        "sales"
    ];

    public ushort PrefetchCount { get; set; } = 20;

    public string ConsumerName { get; set; } =
        "xframework-consumer";

    public bool RequeueOnConsumerShutdown { get; set; } = false;
}
