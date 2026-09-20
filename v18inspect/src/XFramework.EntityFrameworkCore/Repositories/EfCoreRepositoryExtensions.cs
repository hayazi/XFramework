using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Abstractions;

namespace XFramework.EntityFrameworkCore.Repositories;

public static class EfCoreRepositoryExtensions
{
    public static IServiceCollection AddXFrameworkRepositories(
        this IServiceCollection services)
    {
        services.AddScoped(
            typeof(IRepository<,>),
            typeof(EfCoreRepository<,>));

        return services;
    }
}