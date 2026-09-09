using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Abstractions;
using XFramework.EntityFrameworkCore.Repositories;
using XFramework.EntityFrameworkCore.UnitOfWork;

namespace XFramework.EntityFrameworkCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection
        AddXFrameworkEntityFrameworkCore(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddDbContext<ERPDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("Default"));
        });

        services.AddScoped<XFrameworkDbContext>(
            provider =>
                provider.GetRequiredService<ERPDbContext>());

        services.AddScoped(
            typeof(IRepository<,>),
            typeof(EfRepository<,>));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IPermissionRepository, EfCorePermissionRepository>();
        services.AddScoped<IRoleRepository, EfCoreRoleRepository>();

        return services;
    }
}