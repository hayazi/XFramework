namespace XFramework.Application.Exceptions;

public sealed class ValidationException : XFrameworkException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(
        IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
}