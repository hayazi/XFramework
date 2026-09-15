using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using XFramework.Infrastructure.Messaging.RabbitMQ;

namespace XFramework.Infrastructure.DependencyInjection;

public static IServiceCollection
    AddXFrameworkInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
{
    services
        .AddOptions<RabbitMqOptions>()
        .Bind(configuration.GetSection(
            RabbitMqOptions.SectionName))
        .ValidateOnStart();

    services
        .AddOptions<RabbitMqRetryOptions>()
        .Bind(configuration.GetSection(
            RabbitMqRetryOptions.SectionName))
        .ValidateOnStart();

    services.AddSingleton<
        RabbitMqConnectionFactory>();

    services.AddSingleton<
        RabbitMqConnectionManager>();

    services.AddSingleton<
        RabbitMqChannelManager>();

    services.AddSingleton<
        IEventBus,
        RabbitMqEventBus>();

    services.AddSingleton<
        IEventRetryPublisher,
        RabbitMqRetryPublisher>();

    services.AddSingleton<
        IEventDeadLetterPublisher,
        RabbitMqDeadLetterPublisher>();

    return services;
}