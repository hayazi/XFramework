namespace XFramework.Application.Contracts.Authorization;

public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(
        string permissionName,
        CancellationToken cancellationToken = default);
}