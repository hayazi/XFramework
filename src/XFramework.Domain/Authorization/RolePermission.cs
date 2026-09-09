namespace XFramework.Domain.Authorization;

public class RolePermission
{
    public Guid RoleId { get; protected set; }

    public Guid PermissionId { get; protected set; }

    public Role? Role { get; protected set; }

    public Permission? Permission { get; protected set; }

    protected RolePermission()
    {
    }

    public RolePermission(
        Guid roleId,
        Guid permissionId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException(
                "RoleId cannot be empty.",
                nameof(roleId));
        }

        if (permissionId == Guid.Empty)
        {
            throw new ArgumentException(
                "PermissionId cannot be empty.",
                nameof(permissionId));
        }

        RoleId = roleId;
        PermissionId = permissionId;
    }
}