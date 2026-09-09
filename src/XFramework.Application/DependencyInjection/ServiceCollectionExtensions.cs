using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Authorization;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Interceptors;
using XFramework.Application.Services;

namespace XFramework.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXFrameworkApplication(
        this IServiceCollection services)
    {
        // ---------------------------------------------------------
        // Current User
        // ---------------------------------------------------------

        services.AddHttpContextAccessor();

        services.AddScoped<
            ICurrentUser,
            CurrentUser>();

        // ---------------------------------------------------------
        // Authorization
        // ---------------------------------------------------------

        services.AddScoped<
            IPermissionChecker,
            DefaultPermissionChecker>();

        services.AddScoped<
            IPermissionSynchronizer,
            PermissionSynchronizer>();

        // ---------------------------------------------------------
        // Application Service Discovery
        // ---------------------------------------------------------

        RegisterApplicationServices(services);

        // ---------------------------------------------------------
        // Interceptors
        // ---------------------------------------------------------

        services.AddScoped<
            AuthorizationInterceptor>();

        services.AddScoped<
            ValidationInterceptor>();

        services.AddScoped<
            UnitOfWorkInterceptor>();

        return services;
    }

    private static void RegisterApplicationServices(
        IServiceCollection services)
    {
        // Application-service discovery will be implemented
        // by scanning the XFramework.Application assembly.
        //
        // Concrete application services such as:
        //
        // RoleAppService
        // UserAppService
        // CustomerAppService
        //
        // will be registered automatically.

        var assembly =
            typeof(ServiceCollectionExtensions).Assembly;

        var serviceTypes =
            assembly
                .GetTypes()
                .Where(type =>
                    type is { IsClass: true, IsAbstract: false } &&
                    typeof(IApplicationService)
                        .IsAssignableFrom(type));

        foreach (var implementationType in serviceTypes)
        {
            var interfaces =
                implementationType
                    .GetInterfaces()
                    .Where(type =>
                        typeof(IApplicationService)
                            .IsAssignableFrom(type));

            foreach (var serviceType in interfaces)
            {
                services.AddScoped(
                    serviceType,
                    implementationType);
            }
        }
    }
}