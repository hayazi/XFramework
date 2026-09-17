using XFramework.Application.Abstractions;
using XFramework.Application.Contracts.Identity;
using XFramework.Application.Services;
using XFramework.Domain.Identity;

namespace XFramework.Application.Identity;

public sealed class RoleAppService
    : CrudAppService<Role, RoleDto, Guid, RoleCreateDto, RoleUpdateDto>,
      IRoleAppService
{
    private readonly IRoleRepository _roleRepository;

    public RoleAppService(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
        : base(roleRepository, unitOfWork)
    {
        _roleRepository = roleRepository;
    }

    protected override RoleDto MapToDto(Role entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        DisplayName = entity.DisplayName,
        IsSystemRole = entity.IsSystemRole,
        IsActive = entity.IsActive
    };

    protected override Task<Role> MapToEntityAsync(
        RoleCreateDto input,
        CancellationToken ct) =>
        Task.FromResult(
            Role.Create(
                input.Name,
                input.DisplayName,
                input.IsSystemRole));

    protected override Task MapToEntityAsync(
        RoleUpdateDto input,
        Role entity,
        CancellationToken ct)
    {
        entity.Update(
            input.Name,
            input.DisplayName,
            input.IsActive);

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<RolePermissionDto>> GetPermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetAsync(
            roleId,
            cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException(
                $"Role '{roleId}' was not found.");
        }

        var permissions = await _roleRepository.GetPermissionsAsync(
            roleId,
            cancellationToken);

        return permissions
            .Select(permission => new RolePermissionDto
            {
                PermissionId = permission.Id,
                PermissionName = permission.Name,
                DisplayName = permission.DisplayName ?? permission.Name,
                IsGranted = true
            })
            .ToList();
    }

    public async Task SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetAsync(
            roleId,
            cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException(
                $"Role '{roleId}' was not found.");
        }

        await _roleRepository.SetPermissionsAsync(
            roleId,
            permissionIds,
            cancellationToken);
    }
}
