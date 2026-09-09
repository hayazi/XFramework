namespace XFramework.Domain.Identity;

public class User
{
    public Guid Id { get; protected set; }

    public string UserName { get; protected set; }

    public string? Email { get; protected set; }

    public string? FirstName { get; protected set; }

    public string? LastName { get; protected set; }

    public bool IsActive { get; protected set; }

    public bool IsSystemUser { get; protected set; }

    protected User()
    {
        UserName = string.Empty;
    }

    public User(
        string userName,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        bool isSystemUser = false)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException(
                "User name cannot be empty.",
                nameof(userName));
        }

        Id = Guid.NewGuid();
        UserName = userName;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        IsSystemUser = isSystemUser;
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