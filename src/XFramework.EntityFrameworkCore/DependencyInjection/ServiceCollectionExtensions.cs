using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Contracts.Services;
using XFramework.Application.Authorization;
using XFramework.EntityFrameworkCore.Authorization;
using XFramework.EntityFrameworkCore.Repositories;

namespace XFramework.EntityFrameworkCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXFrameworkEntityFrameworkCore(
        this IServiceCollection services,
        //Action<DbContextOptionsBuilder> optionsAction
        string connectionString)
    {
        services.AddScoped<DomainEventToOutboxInterceptor>();

        services.AddDbContext<XFrameworkDbContext>(
            (serviceProvider, options) =>
            {
                var interceptor =
                    serviceProvider.GetRequiredService<
                        DomainEventToOutboxInterceptor>();

                options
                    .UseSqlServer(connectionString)
                    .AddInterceptors(interceptor);
            });
        
        //services.AddDbContext<XFrameworkDbContext>(optionsAction);

        services.AddScoped(typeof(IRepository<,>), typeof(EfCoreRepository<,>));

        services.AddScoped<IPermissionRepository, EfCorePermissionRepository>();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        
        return services;
    }
}