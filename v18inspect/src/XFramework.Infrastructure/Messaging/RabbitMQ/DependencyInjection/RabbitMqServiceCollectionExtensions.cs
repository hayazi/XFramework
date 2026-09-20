using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Events;

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
            .Validate(x => !string.IsNullOrWhiteSpace(x.HostName),
                "RabbitMQ HostName is required.")
            .Validate(x => x.Port > 0,
                "RabbitMQ Port must be greater than zero.")
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqRetryOptions>()
            .BindConfiguration(RabbitMqRetryOptions.SectionName)
            .Validate(x => x.DelaysInSeconds is { Length: > 0 },
                "RabbitMQ retry delays are required.")
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqConsumerOptions>()
            .BindConfiguration(RabbitMqConsumerOptions.SectionName)
            .Validate(x => x.PrefetchCount > 0,
                "RabbitMQ consumer PrefetchCount must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddSingleton<RabbitMqConnectionManager>();
        services.AddSingleton<RabbitMqChannelManager>();
        services.AddSingleton<RabbitMqTopology>();

        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddSingleton<IEventRetryPublisher, RabbitMqRetryPublisher>();
        services.AddSingleton<IEventDeadLetterPublisher, RabbitMqDeadLetterPublisher>();

        // A message handler depends on scoped application services.
        // The hosted consumer resolves it from a scope for each delivery.
        services.AddScoped<RabbitMqMessageHandler>();

        services.AddHostedService<RabbitMqTopologyHostedService>();
        services.AddHostedService<RabbitMqConsumerHostedService>();

        return services;
    }
}
