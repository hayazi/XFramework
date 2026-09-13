namespace XFramework.Application.Exceptions;

public abstract class XFrameworkException : Exception
{
    protected XFrameworkException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}