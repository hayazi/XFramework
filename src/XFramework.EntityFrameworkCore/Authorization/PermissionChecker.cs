using Microsoft.EntityFrameworkCore;
using XFramework.Application.Contracts.Authorization;
using XFramework.EntityFrameworkCore.Identity;
using XFramework.EntityFrameworkCore.Persistence;

namespace XFramework.EntityFrameworkCore.Authorization;

public sealed class PermissionChecker(
    XFrameworkIdentityDbContext db,
    ICurrentUser currentUser)
    : IPermissionChecker
{
    public async Task<bool> IsGrantedAsync(
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
            return false;

        if (!Guid.TryParse(currentUser.UserId, out var userId))
            return false;

        var directPermission =
            await db.ApplicationUserPermissions
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.UserId == userId &&
                        x.Permission.Name == permission &&
                        x.Permission.IsEnabled,
                    cancellationToken);

        if (directPermission)
            return true;

        return await db.ApplicationUserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Role.Permissions)
            .AnyAsync(
                x =>
                    x.Permission.Name == permission &&
                    x.Permission.IsEnabled,
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

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
            return Array.Empty<string>();

        if (!Guid.TryParse(currentUser.UserId, out var userId))
            return Array.Empty<string>();

        var directPermissions =
            await db.ApplicationUserPermissions
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Permission.IsEnabled)
                .Select(x => x.Permission.Name)
                .ToListAsync(cancellationToken);

        var rolePermissions =
            await db.ApplicationUserRoles
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .SelectMany(x => x.Role.Permissions)
                .Where(x => x.Permission.IsEnabled)
                .Select(x => x.Permission.Name)
                .ToListAsync(cancellationToken);

        return directPermissions
            .Union(rolePermissions, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
