using XFramework.Domain.Entities;

namespace XFramework.Domain.Identity;

public sealed class Permission : Entity<Guid>
{
    private readonly List<RolePermission> _roles = new();
    private readonly List<UserPermission> _users = new();
    private Permission() { }

    public string Name { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public string? Description { get; private set; }
    public string? GroupName { get; private set; }
    public bool IsEnabled { get; private set; }
    public IReadOnlyCollection<RolePermission> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<UserPermission> Users => _users.AsReadOnly();

    public static Permission Create(string name, string? displayName = null, string? groupName = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Permission name is required.", nameof(name));
        return new Permission { Id=Guid.NewGuid(), Name=name, DisplayName=displayName, GroupName=groupName, IsEnabled=true };
    }

    public void UpdateDefinition(string displayName, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Permission display name is required.", nameof(displayName));
        DisplayName=displayName; Description=description;
    }

    public void Enable() => IsEnabled=true;
    public void Disable() => IsEnabled=false;
}
