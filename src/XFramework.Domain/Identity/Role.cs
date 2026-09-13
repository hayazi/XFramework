namespace XFramework.Domain.Identity;

public sealed class Role
{
    private readonly List<RolePermission> _permissions = new();

    private Role()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions =>
        _permissions;

    public static Role Create(
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Role name is required.",
                nameof(name));

        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = true
        };
    }
}