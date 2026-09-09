using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Discovery;
using XFramework.Application.Interceptors;
using XFramework.Application.Validation;
using XFramework.Application.Authorization;
using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection
        AddXFrameworkApplication(
            this IServiceCollection services,
            params System.Reflection.Assembly[] assemblies)
    {
        services.AddScoped<UnitOfWorkInterceptor>();

        services.AddScoped<ValidationInterceptor>();

        
        services.AddHttpContextAccessor();

        services.AddScoped<
            ICurrentUser,
            CurrentUser>();

        services.AddScoped<
            IPermissionChecker,
            DefaultPermissionChecker>();

        services.AddScoped<
            IPermissionSynchronizer,
            PermissionSynchronizer>();

        services.AddScoped<
            AuthorizationInterceptor>();


        services.AddSingleton<ProxyGenerator>();

        PermissionProviderDiscovery.Register(
            services,
            assemblies);

        services.AddSingleton<
            PermissionDefinitionRegistry>();

        var descriptors =
            ApplicationServiceDiscovery.Discover(assemblies);

        ApplicationServiceRegistration.Register(
            services,
            descriptors);

        return services;
    }
}