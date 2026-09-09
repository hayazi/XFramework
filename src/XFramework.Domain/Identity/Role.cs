namespace XFramework.Domain.Identity;

public class Role
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    public string? DisplayName { get; protected set; }

    public bool IsSystemRole { get; protected set; }

    public bool IsActive { get; protected set; }

    protected Role()
    {
        Name = string.Empty;
    }

    public Role(
        string name,
        string? displayName = null,
        bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Role name cannot be empty.",
                nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
        DisplayName = displayName;
        IsSystemRole = isSystemRole;
        IsActive = true;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}