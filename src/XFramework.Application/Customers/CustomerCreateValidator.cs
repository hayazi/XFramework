using XFramework.Application.Contracts.Customers;
using XFramework.Application.Validation;

namespace XFramework.Application.Customers;

public sealed class CustomerCreateValidator
    : IValidator<CustomerCreateDto>
{
    public Task<ValidationResult> ValidateAsync(
        CustomerCreateDto input,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(input.Code))
        {
            result.Add(
                nameof(input.Code),
                "Customer code is required.",
                "Customer.Code.Required");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            result.Add(
                nameof(input.Name),
                "Customer name is required.",
                "Customer.Name.Required");
        }

        if (!string.IsNullOrWhiteSpace(input.Name)
            && input.Name.Length > 200)
        {
            result.Add(
                nameof(input.Name),
                "Customer name cannot exceed 200 characters.",
                "Customer.Name.MaxLength");
        }

        return Task.FromResult(result);
    }
}