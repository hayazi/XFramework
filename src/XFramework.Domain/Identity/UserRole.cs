namespace XFramework.Domain.Identity;

public class UserRole
{
    public Guid UserId { get; protected set; }

    public Guid RoleId { get; protected set; }

    public User? User { get; protected set; }

    public Role? Role { get; protected set; }

    protected UserRole()
    {
    }

    public UserRole(
        Guid userId,
        Guid roleId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId cannot be empty.",
                nameof(userId));
        }

        if (roleId == Guid.Empty)
        {
            throw new ArgumentException(
                "RoleId cannot be empty.",
                nameof(roleId));
        }

        UserId = userId;
        RoleId = roleId;
    }
}