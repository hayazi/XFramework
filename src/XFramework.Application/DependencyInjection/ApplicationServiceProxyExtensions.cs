using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace XFramework.Application.DependencyInjection;

public static class ApplicationServiceProxyExtensions
{
    public static IServiceCollection AddInterceptedService<
        TService,
        TImplementation>(
        this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddScoped<TImplementation>();

        services.AddScoped<TService>(static provider =>
        {
            var implementation =
                provider.GetRequiredService<TImplementation>();

            var proxyGenerator =
                new ProxyGenerator();

            IInterceptor[] interceptors = new IInterceptor[]
            {
                provider.GetRequiredService<
                    Interceptors.ValidationInterceptor>(),

                provider.GetRequiredService<
                    Interceptors.UnitOfWorkInterceptor>()
            };            

            return proxyGenerator
                .CreateInterfaceProxyWithTarget<TService>(
                    implementation,
                    interceptors);
        });

        return services;
    }
}