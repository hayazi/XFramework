namespace XFramework.Application.Contracts.Authorization;

public interface IPermissionSynchronizer
{
    Task SynchronizeAsync(
        CancellationToken cancellationToken = default);
}