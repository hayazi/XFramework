using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Interceptors;
using XFramework.Application.Validation;

namespace XFramework.Application.Discovery;

public static class ApplicationServiceRegistration
{
    public static void Register(IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        services.AddSingleton<ProxyGenerator>();

        var descriptors = ApplicationServiceDiscovery.Discover(assembly);

        foreach (var descriptor in descriptors)
        {
            services.AddScoped(descriptor.ImplementationType);

            foreach (var serviceType in descriptor.ServiceTypes)
            {
                services.AddScoped(serviceType, sp =>
                {
                    var target = sp.GetRequiredService(descriptor.ImplementationType);
                    var generator = sp.GetRequiredService<ProxyGenerator>();
                    var interceptors = new IInterceptor[]
                    {
                        sp.GetRequiredService<ApplicationServicePipelineInterceptor>()
                    };

                    return generator.CreateInterfaceProxyWithTarget(
                        serviceType,
                        target,
                        interceptors);
                });
            }
        }

        RegisterValidators(services, assembly);
    }

    private static void RegisterValidators(
        IServiceCollection services,
        System.Reflection.Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            var validatorInterfaces = type.GetInterfaces()
                .Where(x => x.IsGenericType
                    && x.GetGenericTypeDefinition() == typeof(IValidator<>))
                .ToArray();

            foreach (var validatorInterface in validatorInterfaces)
            {
                services.AddScoped(validatorInterface, type);
            }
        }
    }
}
