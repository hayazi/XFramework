using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XFramework.EntityFrameworkCore.DependencyInjection;

public static class EntityFrameworkCoreServiceCollectionExtensions
{
    public static IServiceCollection AddXFrameworkEntityFrameworkCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' was not found.");
        }

        services.AddDbContext<XFrameworkDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));

        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.AddScoped<IIdempotencyService, IdempotencyService>();

        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        return services;
    }
}