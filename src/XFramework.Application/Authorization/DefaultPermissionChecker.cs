using XFramework.Application.Contracts.Authorization;
using XFramework.Domain.Authorization;

namespace XFramework.Application.Authorization;

public sealed class DefaultPermissionChecker : IPermissionChecker
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
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated ||
            !Guid.TryParse(_currentUser.UserId, out var userId) ||
            string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        return await _permissionRepository.IsGrantedAsync(
            userId,
            permission,
            cancellationToken);
    }

    public async Task CheckAsync(
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (!await IsGrantedAsync(permission, cancellationToken))
        {
            throw new AuthorizationException(permission);
        }
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated ||
            !Guid.TryParse(_currentUser.UserId, out var userId))
        {
            return Array.Empty<string>();
        }

        return await _permissionRepository.GetPermissionsAsync(
            userId,
            cancellationToken);
    }
}
