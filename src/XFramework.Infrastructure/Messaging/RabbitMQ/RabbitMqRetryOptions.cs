namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqRetryOptions
{
    public const string SectionName = "RabbitMQ:Retry";

    public int[] DelaysInSeconds { get; set; } =
    {
        5,
        30,
        120,
        600,
        1800
    };
}