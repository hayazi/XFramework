namespace XFramework.Application.Outbox;

public sealed class OutboxOptions
{
    public int BatchSize { get; set; } = 100;
    public int PollingIntervalSeconds { get; set; } = 1;
    public int LeaseMinutes { get; set; } = 2;
    public int MaxRetryCount { get; set; } = 10;
}
