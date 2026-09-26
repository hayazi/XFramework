using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Contracts.Security;
using XFramework.Infrastructure.Authorization;
using XFramework.Infrastructure.Security;
using XFramework.Application.Outbox;
using XFramework.Infrastructure.Outbox;

namespace XFramework.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddXFrameworkInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton<ISecretProvider, ConfigurationSecretProvider>();

        services.AddXFrameworkAuthorization();

        services
            .AddOptions<OutboxOptions>()
            .BindConfiguration("Outbox")
            .Validate(x => x.BatchSize > 0,
                "Outbox BatchSize must be greater than zero.")
            .Validate(x => x.PollingIntervalSeconds > 0,
                "Outbox PollingIntervalSeconds must be greater than zero.")
            .Validate(x => x.LeaseMinutes > 0,
                "Outbox LeaseMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }
}
