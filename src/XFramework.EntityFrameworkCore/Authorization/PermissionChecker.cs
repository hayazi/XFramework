using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Exceptions;

namespace XFramework.EntityFrameworkCore.Authorization;

public sealed class PermissionChecker : IPermissionChecker
{
    public Task<bool> IsGrantedAsync(
        string permission,
        CancellationToken cancellationToken = default)
    {
        // Temporary implementation.
        // Real User/Role/Permission storage will be added
        // in the authorization persistence model.
        return Task.FromResult(false);
    }

    public async Task CheckAsync(
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (!await IsGrantedAsync(permission, cancellationToken))
        {
            throw new ForbiddenException(permission);
        }
    }
}