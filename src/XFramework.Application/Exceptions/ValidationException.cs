using XFramework.Application.Contracts.Validation;

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

    public ValidationException(
        IReadOnlyCollection<ValidationError> errors)
        : this(errors
            .GroupBy(x => x.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.Message).ToArray(),
                StringComparer.Ordinal))
    {
    }
}
