using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Pipelines;

namespace XFramework.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddXFrameworkApplication(this IServiceCollection services)
    {
        services.AddScoped<ApplicationServicePipeline>();
        // services.AddScoped<IApplicationServiceInterceptor, LoggingInterceptor>();
        // services.AddScoped<IApplicationServiceInterceptor, ValidationInterceptor>();
        // services.AddScoped<IApplicationServiceInterceptor, UnitOfWorkInterceptor>();
        services.AddScoped<IApplicationServiceInterceptor, AuthorizationInterceptor>();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditStore, EfAuditStore>();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<XFrameworkDbContext>(
                                                (serviceProvider, options) =>
                                                {
                                                    options.UseSqlServer(connectionString);

                                                    options.AddInterceptors(
                                                        serviceProvider.GetRequiredService<
                                                            AuditSaveChangesInterceptor>());
                                                });
        
        services.AddSingleton<EventTypeRegistry>();

        services.AddSingleton<IEventTypeRegistry>(
            provider =>
                provider.GetRequiredService<EventTypeRegistry>());
                
        var assembly = typeof(DependencyInjection).Assembly;
        foreach (var type in assembly.GetTypes().Where(x => x is { IsClass: true, IsAbstract: false }))
        {
            foreach (var contract in type.GetInterfaces().Where(x => typeof(IApplicationService).IsAssignableFrom(x)))
                services.AddScoped(contract, type);
        }
        return services;
    }
}