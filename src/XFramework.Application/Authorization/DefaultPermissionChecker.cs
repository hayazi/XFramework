using XFramework.Application.Contracts.Authorization;
using XFramework.Domain.Authorization;

namespace XFramework.Application.Authorization;

public sealed class DefaultPermissionChecker
    : IPermissionChecker
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionRepository _permissionRepository;

    public DefaultPermissionChecker(
        ICurrentUser currentUser,
        IPermissionRepository permissionRepository)
    {
        _currentUser = currentUser;
        _permissionRepository = permissionRepository;
    }

    public async Task<bool> IsGrantedAsync(
        string permissionName,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return false;
        }

        if (_currentUser.UserId is not Guid userId)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(permissionName))
        {
            return false;
        }

        return await _permissionRepository.IsGrantedAsync(
            userId,
            permissionName,
            cancellationToken);
    }
}