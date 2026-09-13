namespace XFramework.Application.Exceptions;

public sealed class ForbiddenException : XFrameworkException
{
    public string Permission { get; }

    public ForbiddenException(string permission)
        : base($"Permission '{permission}' is required.")
    {
        Permission = permission;
    }
}