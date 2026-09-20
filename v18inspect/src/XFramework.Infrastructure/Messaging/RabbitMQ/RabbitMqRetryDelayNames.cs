namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public static class RabbitMqRetryDelayNames
{
    public static string FromSeconds(int seconds)
    {
        return seconds switch
        {
            5 => "5s",
            30 => "30s",
            120 => "2m",
            600 => "10m",
            1800 => "30m",
            _ => $"{seconds}s"
        };
    }
}