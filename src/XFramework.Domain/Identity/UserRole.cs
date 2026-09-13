namespace XFramework.Domain.Identity;

public sealed class UserRole
{
    private UserRole()
    {
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public User User { get; private set; } = null!;

    public Role Role { get; private set; } = null!;

    public static UserRole Create(
        Guid userId,
        Guid roleId)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };
    }
}