namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public static class RabbitMqNames
{
    public const string MainExchange = "xframework.events";

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

    public static string RetryRoutingKey(
        string module,
        string delay)
        => $"retry.{module}.{delay}";

    public static string ModuleRoutingKey(
        string module)
        => $"{module}.#";
}