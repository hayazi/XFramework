namespace XFramework.Application.Attributes;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    Inherited = true,
    AllowMultiple = true)]
public sealed class AuthorizeAttribute : Attribute
{
    public string PermissionName { get; }

    public AuthorizeAttribute(string permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
        {
            throw new ArgumentException(
                "Permission name cannot be empty.",
                nameof(permissionName));
        }

        PermissionName = permissionName;
    }
}