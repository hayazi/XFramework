using XFramework.Domain.Authorization;

namespace XFramework.Domain.Identity;
public interface IRoleRepository
    : IRepository<Role, Guid>
{
    Task<IReadOnlyList<Permission>> GetPermissionsAsync(
            Guid roleId,
            CancellationToken cancellationToken = default);

    Task SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default);
}
