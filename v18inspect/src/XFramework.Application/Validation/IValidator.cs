namespace XFramework.Application.Validation;

public interface IValidator<in T>
{
    Task<ValidationResult> ValidateAsync(
        T input,
        CancellationToken cancellationToken = default);
}