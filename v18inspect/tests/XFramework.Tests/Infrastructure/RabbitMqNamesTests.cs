using Xunit;
using XFramework.Infrastructure.Messaging.RabbitMQ;

namespace XFramework.Tests.Infrastructure;

public sealed class RabbitMqNamesTests
{
    [Fact]
    public void MainBindingKey_IsModuleScoped()
    {
        Assert.Equal("inventory.#", RabbitMqNames.MainBindingKey("inventory"));
    }

    [Fact]
    public void RetryBindingKey_IsExactDelayKey()
    {
        Assert.Equal("inventory.retry.30s", RabbitMqNames.RetryBindingKey("inventory", "30s"));
    }

    [Fact]
    public void RetryDelayNames_MapConfiguredDelays()
    {
        Assert.Equal("5s", RabbitMqRetryDelayNames.FromSeconds(5));
        Assert.Equal("30s", RabbitMqRetryDelayNames.FromSeconds(30));
        Assert.Equal("2m", RabbitMqRetryDelayNames.FromSeconds(120));
        Assert.Equal("10m", RabbitMqRetryDelayNames.FromSeconds(600));
        Assert.Equal("30m", RabbitMqRetryDelayNames.FromSeconds(1800));
    }
}
