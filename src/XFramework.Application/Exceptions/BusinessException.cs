namespace XFramework.Application.Exceptions;

public sealed class BusinessException : XFrameworkException
{
    public string Code { get; }

    public object[] Arguments { get; }

    public BusinessException(
        string code,
        params object[] arguments)
        : base(code)
    {
        Code = code;
        Arguments = arguments;
    }
}