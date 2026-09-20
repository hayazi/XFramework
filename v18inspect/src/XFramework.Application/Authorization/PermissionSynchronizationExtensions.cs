using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Authorization;

public static class PermissionSynchronizationExtensions
{
    public static async Task SynchronizePermissionsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope =
            serviceProvider.CreateScope();

        var synchronizer =
            scope.ServiceProvider
                .GetRequiredService<
                    IPermissionSynchronizer>();

        await synchronizer.SynchronizeAsync(
            cancellationToken);
    }
}