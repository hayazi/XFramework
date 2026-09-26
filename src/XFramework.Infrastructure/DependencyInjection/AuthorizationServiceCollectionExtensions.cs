using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Authorization;
using XFramework.Infrastructure.Authorization;

namespace XFramework.Infrastructure.DependencyInjection;

public static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddXFrameworkAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }

    public static AuthorizationPolicyBuilder RequirePermission(
        this AuthorizationPolicyBuilder builder,
        string permissionName)
    {
        return builder.AddRequirements(new PermissionRequirement(permissionName));
    }

    public static IEndpointConventionBuilder RequirePermission(
        this IEndpointConventionBuilder builder,
        string permissionName)
    {
        return builder.RequireAuthorization($"Permission.{permissionName}");
    }
}