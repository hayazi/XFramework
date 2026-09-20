using XFramework.Domain.Entities;

namespace XFramework.Domain.Identity;

public sealed class User : Entity<Guid>
{
    private readonly List<UserRole> _roles = new();
    private readonly List<UserPermission> _permissions = new();
    private User() { }

    public string UserName { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<UserPermission> Permissions => _permissions.AsReadOnly();

    public static User Create(string userName, string? displayName=null)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("User name is required.", nameof(userName));
        return new User { Id=Guid.NewGuid(), UserName=userName, DisplayName=displayName, IsActive=true, CreatedAt=DateTime.UtcNow };
    }

    public void Activate() => IsActive=true;
    public void Deactivate() => IsActive=false;
}
