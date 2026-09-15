namespace XFramework.Application.Events;

public sealed class DefaultEventRetryPolicy
    : IEventRetryPolicy
{
    private const int MaxRetries = 5;

    public bool IsRetryable(Exception exception)
    {
        return exception is TimeoutException
            || exception is HttpRequestException;
    }

    public bool ShouldRetry(
        int retryCount,
        Exception exception)
    {
        return retryCount < MaxRetries
               && IsRetryable(exception);
    }

    public TimeSpan GetDelay(int retryCount)
    {
        return retryCount switch
        {
            0 => TimeSpan.FromSeconds(5),
            1 => TimeSpan.FromSeconds(30),
            2 => TimeSpan.FromMinutes(2),
            3 => TimeSpan.FromMinutes(10),
            4 => TimeSpan.FromMinutes(30),
            _ => TimeSpan.Zero
        };
    }
}