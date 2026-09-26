namespace XFramework.Domain.Authorization;

public interface IPermissionRepository
{
    Task<bool> IsGrantedAsync(Guid userId, string permissionName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
