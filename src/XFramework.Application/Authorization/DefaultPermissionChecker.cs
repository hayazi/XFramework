using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Authorization;

public sealed class DefaultPermissionChecker
    : IPermissionChecker
{
    private readonly PermissionDefinitionRegistry _registry;

    public DefaultPermissionChecker(
        PermissionDefinitionRegistry registry)
    {
        _registry = registry;
    }

    public Task<bool> IsGrantedAsync(
        string permissionName,
        CancellationToken cancellationToken = default)
    {
        var permission =
            _registry.GetPermission(permissionName);

        if (permission is null)
        {
            return Task.FromResult(false);
        }

        // Temporary:
        // Permission exists, but user authorization
        // is not implemented yet.
        return Task.FromResult(false);
    }
}