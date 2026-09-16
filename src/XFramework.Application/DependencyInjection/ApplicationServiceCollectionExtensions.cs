using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Events;
using XFramework.Application.Idempotency;
using XFramework.Application.Messaging;
using XFramework.Application.Services;
using XFramework.Application.Authorization;

namespace XFramework.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddXFramework(
        this IServiceCollection services)
    {
        services.AddApplicationServices();

        services.AddApplicationInfrastructure();

        return services;
    }

    private static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<ICrudAppService, CrudAppService>();

        return services;
    }

    private static IServiceCollection AddApplicationInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IEventProcessor, EventProcessor>();

        services.AddSingleton<IEventTypeRegistry, EventTypeRegistry>();

        services.AddSingleton<IEventRoutingResolver, EventRoutingResolver>();

        services.AddSingleton<IEventRetryPolicy, EventRetryPolicy>();

        return services;
    }
}