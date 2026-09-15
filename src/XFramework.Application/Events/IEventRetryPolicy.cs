namespace XFramework.Application.Events;

public interface IEventRetryPolicy
{
    bool IsRetryable(Exception exception);

    bool ShouldRetry(
        int retryCount,
        Exception exception);

    TimeSpan GetDelay(int retryCount);
}