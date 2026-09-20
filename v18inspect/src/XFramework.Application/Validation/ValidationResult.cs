namespace XFramework.Application.Validation;

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = [];

    public IReadOnlyList<ValidationError> Errors => _errors;

    public bool IsValid => _errors.Count == 0;

    public void Add(
        string propertyName,
        string message,
        string? code = null)
    {
        _errors.Add(
            new ValidationError(
                propertyName,
                message,
                code));
    }

    public static ValidationResult Success()
    {
        return new ValidationResult();
    }
}

public sealed record ValidationError(
    string PropertyName,
    string Message,
    string? Code = null);