using Xunit;
using XFramework.Infrastructure.Messaging.RabbitMQ;

namespace XFramework.Tests.Infrastructure;

public sealed class RabbitMqConfigurationTests
{
    [Fact]
    public void DefaultOptions_AreSafeForLocalDevelopment()
    {
        var options = new RabbitMqOptions();

        Assert.Equal("localhost", options.HostName);
        Assert.Equal(5672, options.Port);
        Assert.Equal("xframework.events", options.ExchangeName);
        Assert.True(options.Durable);
        Assert.True(options.PublisherConfirms);
    }

    [Fact]
    public void ConsumerDefaults_ContainCoreModules()
    {
        var options = new RabbitMqConsumerOptions();

        Assert.Contains("accounting", options.Modules);
        Assert.Contains("inventory", options.Modules);
        Assert.Contains("sales", options.Modules);
        Assert.True(options.PrefetchCount > 0);
    }
}
