namespace XFramework.Application.Events;

public sealed class EventRetryPolicy : IEventRetryPolicy
{
    private static readonly TimeSpan[] RetryDelays =
    {
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(30)
    };

    public bool IsRetryable(Exception exception)
    {
        if (exception is NonRetryableEventException)
            return false;

        return exception switch
        {
            TimeoutException => true,
            HttpRequestException => true,
            _ => IsTransientDatabaseException(exception)
        };
    }

    public bool ShouldRetry(
        int retryCount,
        Exception exception)
    {
        if (!IsRetryable(exception))
            return false;

        return retryCount < RetryDelays.Length;
    }

    public TimeSpan GetDelay(int retryCount)
    {
        if (retryCount < 0)
            retryCount = 0;

        if (retryCount >= RetryDelays.Length)
            return RetryDelays[^1];

        return RetryDelays[retryCount];
    }

    private static bool IsTransientDatabaseException(
        Exception exception)
    {
        // EF Core / SQL transient-error detection
        // will be integrated here later.

        return false;
    }
}