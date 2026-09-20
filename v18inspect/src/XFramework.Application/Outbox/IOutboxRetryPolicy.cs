namespace XFramework.Application.Outbox;
public interface IOutboxRetryPolicy
{
    bool ShouldRetry(
        int retryCount,
        Exception exception);

    TimeSpan GetDelay(
        int retryCount);
}