using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Services;

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