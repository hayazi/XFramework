namespace XFramework.Application.Exceptions;

public class BusinessException : XFrameworkException
{
    public string Code { get; }

    public BusinessException(
        string code,
        string message)
        : base(message)
    {
        Code = code;
    }
}