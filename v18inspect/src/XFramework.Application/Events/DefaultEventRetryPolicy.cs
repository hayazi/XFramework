namespace XFramework.Application.Events;
public sealed class DefaultEventRetryPolicy : IEventRetryPolicy
{
    private static readonly TimeSpan[] Delays={TimeSpan.FromSeconds(5),TimeSpan.FromSeconds(30),TimeSpan.FromMinutes(2),TimeSpan.FromMinutes(10),TimeSpan.FromMinutes(30)};
    public bool IsRetryable(Exception ex)=>ex is TimeoutException or HttpRequestException || ex.InnerException is TimeoutException or HttpRequestException;
    public bool ShouldRetry(int retryCount,Exception ex)=>IsRetryable(ex)&&retryCount<Delays.Length;
    public TimeSpan GetDelay(int retryCount)=>retryCount>=0&&retryCount<Delays.Length?Delays[retryCount]:Delays[^1];
}
