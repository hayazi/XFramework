namespace XFramework.Application.Contracts.Authorization;

public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(
        string permission,
        CancellationToken cancellationToken = default);

    Task CheckAsync(
        string permission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPermissionsAsync(
        CancellationToken cancellationToken = default);
}