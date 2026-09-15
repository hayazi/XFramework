using XFramework.Application.Outbox;
namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxRetryPolicy :IOutboxRetryPolicy
{
    public const int MaxRetryCount = 10;

    public bool CanRetry(int retryCount)
    {
        return retryCount < MaxRetryCount;
    }

    public TimeSpan GetDelay(int retryCount)
    {
        var seconds = Math.Min(
            Math.Pow(2, retryCount),
            300);

        return TimeSpan.FromSeconds(seconds);
    }
}