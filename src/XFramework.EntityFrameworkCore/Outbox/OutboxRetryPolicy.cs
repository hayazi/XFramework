using XFramework.Application.Outbox;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxRetryPolicy : IOutboxRetryPolicy
{
    private static readonly TimeSpan[] Delays =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(30)
    ];

    public bool ShouldRetry(
        int retryCount,
        Exception exception)
    {
        if (retryCount >= Delays.Length)
        {
            return false;
        }

        return exception is TimeoutException
            or HttpRequestException
            or TaskCanceledException;
    }

    public TimeSpan GetDelay(int retryCount)
    {
        if (retryCount < 0)
        {
            return Delays[0];
        }

        return retryCount < Delays.Length
            ? Delays[retryCount]
            : Delays[^1];
    }
}
