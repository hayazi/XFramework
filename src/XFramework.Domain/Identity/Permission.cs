namespace XFramework.Domain.Identity;

public sealed class Permission
{
    private Permission()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? DisplayName { get; private set; }

    public string? GroupName { get; private set; }

    public bool IsActive { get; private set; }

    public static Permission Create(
        string name,
        string? displayName = null,
        string? groupName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Permission name is required.",
                nameof(name));

        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            GroupName = groupName,
            IsActive = true
        };
    }
}