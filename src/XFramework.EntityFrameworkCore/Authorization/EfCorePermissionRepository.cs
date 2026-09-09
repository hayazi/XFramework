using Microsoft.EntityFrameworkCore;
using XFramework.Domain.Authorization;
using XFramework.EntityFrameworkCore.Identity;

namespace XFramework.EntityFrameworkCore.Authorization;

public sealed class EfCorePermissionRepository
    : IPermissionRepository
{
    private readonly XFrameworkIdentityDbContext _dbContext;

    public EfCorePermissionRepository(
        XFrameworkIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsGrantedAsync(
        Guid userId,
        string permissionName,
        CancellationToken cancellationToken = default)
    {
        return await
            (
                from userRole in _dbContext.UserRoles
                join rolePermission in
                    _dbContext.RolePermissions
                    on userRole.RoleId
                    equals rolePermission.RoleId

                join permission in
                    _dbContext.Permissions
                    on rolePermission.PermissionId
                    equals permission.Id

                join role in
                    _dbContext.Roles
                    on userRole.RoleId
                    equals role.Id

                where
                    userRole.UserId == userId
                    && role.IsActive
                    && permission.IsEnabled
                    && permission.Name == permissionName

                select permission.Id
            )
            .AnyAsync(cancellationToken);
    }
}