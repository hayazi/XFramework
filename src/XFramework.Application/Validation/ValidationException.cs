namespace XFramework.Application.Validation;

public sealed class ValidationException
    : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(
        IReadOnlyList<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}