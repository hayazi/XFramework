using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Authorization;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Discovery;
using XFramework.Application.Events;
using XFramework.Application.Interceptors;

namespace XFramework.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddXFramework(
        this IServiceCollection services)
    {
        services.AddScoped<IPermissionChecker, DefaultPermissionChecker>();
        services.AddScoped<IEventProcessor, EventProcessor>();

        services.AddScoped<IApplicationServiceInterceptor, LoggingApplicationServiceInterceptor>();
        services.AddScoped<IApplicationServiceInterceptor, AuthorizationApplicationServiceInterceptor>();
        services.AddScoped<IApplicationServiceInterceptor, ValidationApplicationServiceInterceptor>();
        services.AddScoped<IApplicationServiceInterceptor, UnitOfWorkApplicationServiceInterceptor>();
        services.AddScoped<IApplicationServiceInterceptor, AuditApplicationServiceInterceptor>();
        services.AddScoped<ApplicationServicePipelineInterceptor>();

        services.AddSingleton<EventTypeRegistry>();
        services.AddSingleton<IEventTypeRegistry>(
            sp => sp.GetRequiredService<EventTypeRegistry>());
        services.AddSingleton<IEventSerializer, EventSerializer>();
        services.AddSingleton<IEventRoutingResolver, EventRoutingResolver>();
        services.AddSingleton<IEventRetryPolicy, DefaultEventRetryPolicy>();

        ApplicationServiceRegistration.Register(services);

        return services;
    }
}
