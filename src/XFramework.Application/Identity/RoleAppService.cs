using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Contracts.Identity;
using XFramework.Domain.Identity;

namespace XFramework.Application.Identity;

public sealed class RoleAppService
    : CrudAppService<
        Role,
        RoleDto,
        Guid,
        RoleCreateDto,
        RoleUpdateDto>,
      IRoleAppService
{
    private readonly IRoleRepository _roleRepository;

    public RoleAppService(
        ICurrentUser currentUser,
        IRoleRepository roleRepository)
        : base(
            currentUser,
            roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IReadOnlyList<RolePermissionDto>>
        GetPermissionsAsync(
            Guid roleId,
            CancellationToken cancellationToken = default)
    {
        var role =
            await _roleRepository.GetAsync(
                roleId,
                cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException(
                $"Role '{roleId}' was not found.");
        }

        var permissions =
            await _roleRepository.GetPermissionsAsync(
                roleId,
                cancellationToken);

        return permissions
            .Select(permission =>
                new RolePermissionDto
                {
                    PermissionId = permission.Id,
                    PermissionName = permission.Name,
                    DisplayName = permission.DisplayName,
                    IsGranted = true
                })
            .ToList();
    }

    public async Task SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        var role =
            await _roleRepository.GetAsync(
                roleId,
                cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException(
                $"Role '{roleId}' was not found.");
        }

        await _roleRepository.SetPermissionsAsync(
            roleId,
            permissionIds,
            cancellationToken);
    }
}