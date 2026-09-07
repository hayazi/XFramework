namespace XFramework.Application.Authorization;

public sealed class AuthorizationException : Exception
{
    public string PermissionName { get; }

    public AuthorizationException(
        string permissionName)
        : base(
            $"The current user is not authorized to perform this operation. " +
            $"Required permission: '{permissionName}'.")
    {
        PermissionName = permissionName;
    }
}