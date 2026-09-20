using Xunit;
using XFramework.Application.Events;

namespace XFramework.Tests.Infrastructure;

public sealed class EventRetryPolicyTests
{
    private readonly DefaultEventRetryPolicy _policy = new();

    [Fact]
    public void Timeout_IsRetryable()
    {
        Assert.True(_policy.IsRetryable(new TimeoutException()));
    }

    [Fact]
    public void HttpRequest_IsRetryable()
    {
        Assert.True(_policy.IsRetryable(new HttpRequestException()));
    }

    [Fact]
    public void BusinessException_IsNotRetryable()
    {
        Assert.False(_policy.IsRetryable(new InvalidOperationException("business failure")));
    }

    [Fact]
    public void RetryStopsAfterConfiguredAttempts()
    {
        var exception = new TimeoutException();

        Assert.True(_policy.ShouldRetry(0, exception));
        Assert.True(_policy.ShouldRetry(4, exception));
        Assert.False(_policy.ShouldRetry(5, exception));
    }

    [Fact]
    public void Delays_AreStableAndOrdered()
    {
        Assert.Equal(TimeSpan.FromSeconds(5), _policy.GetDelay(0));
        Assert.Equal(TimeSpan.FromSeconds(30), _policy.GetDelay(1));
        Assert.Equal(TimeSpan.FromMinutes(2), _policy.GetDelay(2));
        Assert.Equal(TimeSpan.FromMinutes(10), _policy.GetDelay(3));
        Assert.Equal(TimeSpan.FromMinutes(30), _policy.GetDelay(4));
    }
}
