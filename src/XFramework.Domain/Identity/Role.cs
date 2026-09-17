using XFramework.Domain.Entities;

namespace XFramework.Domain.Identity;

public sealed class Role : Entity<Guid>
{
    private readonly List<RolePermission> _permissions = new();
    private Role() { }

    public string Name { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public bool IsSystemRole { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    public static Role Create(string name, string? displayName = null, bool isSystemRole=false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.", nameof(name));
        return new Role { Id=Guid.NewGuid(), Name=name, DisplayName=displayName, IsSystemRole=isSystemRole, IsActive=true };
    }

    public void Update(string name, string? displayName, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.", nameof(name));
        Name=name; DisplayName=displayName; IsActive=isActive;
    }
}
