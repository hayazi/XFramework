using Microsoft.EntityFrameworkCore;
using XFramework.Application.Contracts.Authorization;
using XFramework.Domain.Authorization;
using XFramework.EntityFrameworkCore.Identity;

namespace XFramework.Application.Authorization;

public sealed class PermissionSynchronizer
    : IPermissionSynchronizer
{
    private readonly PermissionDefinitionRegistry _registry;
    private readonly XFrameworkIdentityDbContext _dbContext;

    public PermissionSynchronizer(
        PermissionDefinitionRegistry registry,
        XFrameworkIdentityDbContext dbContext)
    {
        _registry = registry;
        _dbContext = dbContext;
    }

    public async Task SynchronizeAsync(
        CancellationToken cancellationToken = default)
    {
        var definitions =
            GetAllDefinitions();

        var existingPermissions =
            await _dbContext.Permissions
                .ToDictionaryAsync(
                    x => x.Name,
                    StringComparer.OrdinalIgnoreCase,
                    cancellationToken);

        foreach (var definition in definitions)
        {
            if (existingPermissions.TryGetValue(
                    definition.Name,
                    out var existing))
            {
                UpdatePermission(
                    existing,
                    definition);

                continue;
            }

            var permission =
                new Permission(
                    definition.Name,
                    definition.DisplayName);

            await _dbContext.Permissions.AddAsync(
                permission,
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private IReadOnlyList<PermissionDefinition>
        GetAllDefinitions()
    {
        var result =
            new List<PermissionDefinition>();

        foreach (var group in _registry.GetGroups())
        {
            AddChildren(
                group,
                result);
        }

        return result;
    }

    private static void AddChildren(
        PermissionDefinition parent,
        List<PermissionDefinition> result)
    {
        foreach (var child in parent.Children)
        {
            result.Add(child);

            AddChildren(
                child,
                result);
        }
    }

    private static void UpdatePermission(
        Permission permission,
        PermissionDefinition definition)
    {
        permission.UpdateDefinition(
            definition.DisplayName);
    }
}