using Microsoft.EntityFrameworkCore;
using XFramework.Domain.Authorization;
using XFramework.Domain.Identity;

namespace XFramework.EntityFrameworkCore.Identity;

public sealed class EfCoreRoleRepository
    : IRoleRepository
{
    private readonly XFrameworkIdentityDbContext _dbContext;

    public EfCoreRoleRepository(
        XFrameworkIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Role?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetListAsync(
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Role> query =
            _dbContext.Roles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search) ||
                (x.DisplayName != null &&
                 x.DisplayName.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(
                x => x.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Role> query =
            _dbContext.Roles;

        query = query.Where(
            x => x.Name == name);

        if (excludingId.HasValue)
        {
            query = query.Where(
                x => x.Id != excludingId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Roles.AddAsync(
            role,
            cancellationToken);
    }

    public Task UpdateAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Roles.Update(role);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Roles.Remove(role);

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Permission>>
        GetPermissionsAsync(
            Guid roleId,
            CancellationToken cancellationToken = default)
    {
        return await
            (
                from rolePermission
                    in _dbContext.RolePermissions

                join permission
                    in _dbContext.Permissions
                    on rolePermission.PermissionId
                    equals permission.Id

                where rolePermission.RoleId == roleId

                orderby permission.Name

                select permission
            )
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _dbContext.RolePermissions
                .Where(x => x.RoleId == roleId)
                .ToListAsync(cancellationToken);

        _dbContext.RolePermissions.RemoveRange(existing);

        var newPermissions =
            permissionIds
                .Distinct()
                .Select(permissionId =>
                    new RolePermission(
                        roleId,
                        permissionId))
                .ToList();

        await _dbContext.RolePermissions.AddRangeAsync(
            newPermissions,
            cancellationToken);
    }
}