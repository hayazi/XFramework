using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Contracts.Identity;

public interface IRoleAppService
    : ICrudAppService<
        RoleDto,
        Guid,
        PagedRequest,
        RoleCreateDto,
        RoleUpdateDto>
{
    Task<IReadOnlyList<RolePermissionDto>>
        GetPermissionsAsync(
            Guid roleId,
            CancellationToken cancellationToken = default);

    Task SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default);
}