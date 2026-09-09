using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Identity;

public sealed class RolePermissionDefinitionProvider
    : IPermissionDefinitionProvider
{
    public void Define(
        IPermissionDefinitionContext context)
    {
        var identity =
            context.AddGroup(
                "Identity",
                "Identity Management");

        var role =
            context.AddPermission(
                "Identity.Role",
                "Roles",
                identity);

        context.AddPermission(
            "Identity.Role.View",
            "View",
            role);

        context.AddPermission(
            "Identity.Role.Create",
            "Create",
            role);

        context.AddPermission(
            "Identity.Role.Edit",
            "Edit",
            role);

        context.AddPermission(
            "Identity.Role.Delete",
            "Delete",
            role);

        context.AddPermission(
            "Identity.Role.ManagePermissions",
            "Manage Permissions",
            role);
    }
}