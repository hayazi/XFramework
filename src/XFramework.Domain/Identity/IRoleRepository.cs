using XFramework.Domain.Authorization;

namespace XFramework.Domain.Identity;

public interface IRoleRepository
{
    Task<Role?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Role>> GetListAsync(
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>>
        GetPermissionsAsync(
            Guid roleId,
            CancellationToken cancellationToken = default);

    Task SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default);
}