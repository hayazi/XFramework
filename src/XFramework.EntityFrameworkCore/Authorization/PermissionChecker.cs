using Microsoft.EntityFrameworkCore;
using XFramework.Application.Contracts.Authorization;

namespace XFramework.EntityFrameworkCore.Authorization;

public sealed class PermissionChecker(
    XFrameworkDbContext db,
    ICurrentUser currentUser)
    : IPermissionChecker
{
    public async Task<bool> IsGrantedAsync(
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
            return false;

        var userId = currentUser.UserId.Value;

        var directPermission =
            await db.UserPermissions
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.UserId == userId &&
                        x.Permission.Name == permission &&
                        x.Permission.IsActive,
                    cancellationToken);

        if (directPermission)
            return true;

        return await db.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Role.Permissions)
            .AnyAsync(
                x =>
                    x.Permission.Name == permission &&
                    x.Permission.IsActive,
                cancellationToken);
    }

    public async Task CheckAsync(
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (!await IsGrantedAsync(
                permission,
                cancellationToken))
        {
            throw new Application.Exceptions.ForbiddenException(
                permission);
        }
    }
}