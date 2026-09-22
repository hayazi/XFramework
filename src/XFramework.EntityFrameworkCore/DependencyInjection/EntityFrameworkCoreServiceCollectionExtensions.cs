using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Abstractions;
using XFramework.Application.Events;
using XFramework.Application.Outbox;
using XFramework.Domain.Authorization;
using XFramework.EntityFrameworkCore.Authorization;
using XFramework.EntityFrameworkCore.Identity;
using XFramework.EntityFrameworkCore.Idempotency;
using XFramework.EntityFrameworkCore.Outbox;
using XFramework.EntityFrameworkCore.Persistence;
using XFramework.EntityFrameworkCore.Repositories;
using XFramework.EntityFrameworkCore.UnitOfWork;

namespace XFramework.EntityFrameworkCore.DependencyInjection;

public static class EntityFrameworkCoreServiceCollectionExtensions
{
    public static IServiceCollection AddXFrameworkEntityFrameworkCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' was not found.");

        services.AddScoped<DomainEventToOutboxInterceptor>();

        services.AddDbContext<XFrameworkDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<DomainEventToOutboxInterceptor>());
        });

        services.AddDbContext<XFrameworkIdentityDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped(typeof(IRepository<,>), typeof(EfCoreRepository<,>));
        services.AddScoped<IRoleRepository, EfCoreRoleRepository>();
        services.AddScoped<IPermissionRepository, EfCorePermissionRepository>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();
        services.AddSingleton<IOutboxRetryPolicy, OutboxRetryPolicy>();


        return services;
    }
}
