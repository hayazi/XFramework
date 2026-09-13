namespace XFramework.Application.Exceptions;

public sealed class ForbiddenException : XFrameworkException
{
    public string Permission { get; }

    public ForbiddenException(string permission)
        : base("Authorization.Forbidden")
    {
        Permission = permission;
    }
}