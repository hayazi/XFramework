using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace XFramework.Application.Discovery;

internal static class ApplicationServiceRegistration
{
    public static void Register(
        IServiceCollection services,
        IReadOnlyList<ApplicationServiceDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
        {
            RegisterImplementation(
                services,
                descriptor);

            foreach (var serviceType in descriptor.ServiceTypes)
            {
                RegisterService(
                    services,
                    serviceType,
                    descriptor);
            }
        }
    }

    private static void RegisterImplementation(
        IServiceCollection services,
        ApplicationServiceDescriptor descriptor)
    {
        services.AddScoped(
            descriptor.ImplementationType);
    }

    private static void RegisterService(
        IServiceCollection services,
        Type serviceType,
        ApplicationServiceDescriptor descriptor)
    {
        services.AddScoped(
            serviceType,
            provider =>
            {
                var implementation =
                    provider.GetRequiredService(
                        descriptor.ImplementationType);

                var proxyGenerator =
                    provider.GetRequiredService<ProxyGenerator>();

                var interceptors =
                    new IInterceptor[]
                    {
                        provider.GetRequiredService<
                            Interceptors.AuthorizationInterceptor>(),
                            
                        provider.GetRequiredService<
                            Interceptors.ValidationInterceptor>(),

                        provider.GetRequiredService<
                            Interceptors.UnitOfWorkInterceptor>()
                    };

                return proxyGenerator
                    .CreateInterfaceProxyWithTarget(
                        serviceType,
                        implementation,
                        interceptors);
            });
    }
}