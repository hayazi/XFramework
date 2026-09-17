namespace XFramework.Domain.Authorization;

public interface IPermissionRepository
{
    Task<bool> IsGrantedAsync(Guid userId, string permissionName, CancellationToken cancellationToken = default);
}
