using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Authorization;

public sealed class PermissionDefinitionContext
    : IPermissionDefinitionContext
{
    private readonly Dictionary<string, PermissionDefinition>
        _permissions =
            new(StringComparer.OrdinalIgnoreCase);

    private readonly List<PermissionDefinition>
        _groups = [];

    public PermissionDefinition AddGroup(
        string name,
        string displayName)
    {
        if (_permissions.ContainsKey(name))
        {
            throw new InvalidOperationException(
                $"Permission '{name}' is already defined.");
        }

        var group =
            new PermissionDefinition(
                name,
                displayName);

        _permissions.Add(
            name,
            group);

        _groups.Add(group);

        return group;
    }

    public PermissionDefinition AddPermission(
        string name,
        string displayName,
        PermissionDefinition? parent = null)
    {
        if (_permissions.ContainsKey(name))
        {
            throw new InvalidOperationException(
                $"Permission '{name}' is already defined.");
        }

        var permission =
            new PermissionDefinition(
                name,
                displayName,
                parent);

        _permissions.Add(
            name,
            permission);

        parent?.AddChild(permission);

        return permission;
    }

    public PermissionDefinition? GetPermission(
        string name)
    {
        _permissions.TryGetValue(
            name,
            out var permission);

        return permission;
    }

    public IReadOnlyList<PermissionDefinition>
        GetGroups()
    {
        return _groups;
    }
}