namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public static class RabbitMqNames
{
    public const string MainExchange =
        "xframework.events";

    public const string RetryExchange =
        "xframework.events.retry";

    public const string DeadLetterExchange =
        "xframework.events.dlx";

    public static string Queue(string module)
        => $"rgre.{module}.events";

    public static string DeadLetterQueue(string module)
        => $"rgre.{module}.events.dlq";

    public static string RetryQueue(
        string module,
        string delay)
        => $"rgre.{module}.events.retry.{delay}";

    public static string MainBindingKey(
        string module)
        => $"{module}.#";

    public static string RetryBindingKey(
        string module,
        string delay)
        => $"{module}.retry.{delay}";

    public static string RetryPublishKey(
        string module,
        string delay)
        => $"{module}.retry.{delay}";
}