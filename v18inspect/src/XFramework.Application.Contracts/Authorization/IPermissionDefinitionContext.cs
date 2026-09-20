namespace XFramework.Application.Contracts.Authorization;

public interface IPermissionDefinitionContext
{
    PermissionDefinition AddGroup(
        string name,
        string displayName);

    PermissionDefinition AddPermission(
        string name,
        string displayName,
        PermissionDefinition? parent = null);

    PermissionDefinition? GetPermission(
        string name);

    IReadOnlyList<PermissionDefinition> GetGroups();
}