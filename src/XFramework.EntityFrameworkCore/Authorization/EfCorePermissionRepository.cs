using Microsoft.EntityFrameworkCore;
using XFramework.Domain.Authorization;
using XFramework.EntityFrameworkCore.Identity;

namespace XFramework.EntityFrameworkCore.Authorization;

public sealed class EfCorePermissionRepository(XFrameworkIdentityDbContext db) : IPermissionRepository
{
    public Task<bool> IsGrantedAsync(Guid userId, string permissionName, CancellationToken ct = default)
        => (from ur in db.ApplicationUserRoles
            join rp in db.ApplicationRolePermissions on ur.RoleId equals rp.RoleId
            join p in db.ApplicationPermissions on rp.PermissionId equals p.Id
            join r in db.ApplicationRoles on ur.RoleId equals r.Id
            where ur.UserId == userId && r.IsActive && p.IsEnabled && p.Name == permissionName
            select p.Id).AnyAsync(ct);

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var directPermissions = await db.ApplicationUserPermissions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Permission.IsEnabled)
            .Select(x => x.Permission.Name)
            .ToListAsync(ct);

        var rolePermissions = await db.ApplicationUserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Role.Permissions)
            .Where(x => x.Permission.IsEnabled)
            .Select(x => x.Permission.Name)
            .ToListAsync(ct);

        return directPermissions
            .Union(rolePermissions, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
