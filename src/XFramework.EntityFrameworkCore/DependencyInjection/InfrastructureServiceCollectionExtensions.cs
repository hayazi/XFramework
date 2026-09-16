using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XFramework.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddXFrameworkInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddXFrameworkRabbitMQ(configuration);

        return services;
    }
}