namespace XFramework.Application.Contracts.Authorization;

public sealed class PermissionDefinition
{
    private readonly List<PermissionDefinition> _children = [];

    public string Name { get; }

    public string DisplayName { get; }

    public PermissionDefinition? Parent { get; }

    public IReadOnlyList<PermissionDefinition> Children =>
        _children;

    public PermissionDefinition(
        string name,
        string displayName,
        PermissionDefinition? parent = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Permission name cannot be empty.",
                nameof(name));
        }

        Name = name;
        DisplayName = displayName;
        Parent = parent;
    }

    public void AddChild(PermissionDefinition permission)
    {
        ArgumentNullException.ThrowIfNull(permission);
        _children.Add(permission);
    }
}