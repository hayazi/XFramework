namespace XFramework.Domain.Authorization;

public class Permission
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    public string DisplayName { get; protected set; }

    public string? Description { get; protected set; }

    public bool IsEnabled { get; protected set; }

    protected Permission()
    {
        Name = string.Empty;
        DisplayName = string.Empty;
    }

    public Permission(
        string name,
        string displayName,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Permission name cannot be empty.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "Permission display name cannot be empty.",
                nameof(displayName));
        }

        Id = Guid.NewGuid();
        Name = name;
        DisplayName = displayName;
        Description = description;
        IsEnabled = true;
    }
    public void UpdateDefinition(
    string displayName,
    string? description = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "Permission display name cannot be empty.",
                nameof(displayName));
        }

        DisplayName = displayName;
        Description = description;
    }
    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}