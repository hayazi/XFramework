using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XFramework.Infrastructure.Messaging.RabbitMQ.DependencyInjection;

public static class RabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddXFrameworkRabbitMQ(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.HostName),
                "RabbitMQ HostName is required.")
            .Validate(
                options => options.Port > 0,
                "RabbitMQ Port must be greater than zero.")
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqRetryOptions>()
            .BindConfiguration(RabbitMqRetryOptions.SectionName)
            .Validate(
                options =>
                    options.DelaysInSeconds is { Length: > 0 },
                "RabbitMQ retry delays are required.")
            .ValidateOnStart();

        services.AddSingleton<RabbitMqConnectionFactory>();

        services.AddSingleton<RabbitMqConnectionManager>();

        services.AddSingleton<RabbitMqChannelManager>();

        services.AddSingleton<RabbitMqTopology>();

        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        services.AddSingleton<IEventRetryPublisher,
            RabbitMqRetryPublisher>();

        services.AddSingleton<IEventDeadLetterPublisher,
            RabbitMqDeadLetterPublisher>();

        services.AddHostedService<RabbitMqTopologyHostedService>();

        services.AddHostedService<RabbitMqConsumerHostedService>();

        return services;
    }
}