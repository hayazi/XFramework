namespace XFramework.Application.Exceptions;

public sealed class UnauthorizedException : XFrameworkException
{
    public UnauthorizedException(
        string message = "User is not authorized.")
        : base(message)
    {
    }
}