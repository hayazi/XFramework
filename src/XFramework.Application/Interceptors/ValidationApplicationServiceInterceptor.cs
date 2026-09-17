using XFramework.Application.Exceptions;
using XFramework.Application.Validation;

namespace XFramework.Application.Interceptors;

public sealed class ValidationApplicationServiceInterceptor(
    IServiceProvider serviceProvider)
    : IApplicationServiceInterceptor
{
    public async Task<object?> InterceptAsync(
        ApplicationServiceInvocationContext context,
        Func<Task<object?>> next,
        CancellationToken cancellationToken = default)
    {
        if (!context.RequiresValidation)
            return await next().ConfigureAwait(false);

        foreach (var argument in context.Arguments)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            var validator = serviceProvider.GetService(validatorType);

            if (validator is null)
                continue;

            var validateMethod = validatorType.GetMethod(nameof(IValidator<object>.ValidateAsync))!;
            var validationTask = (Task<ValidationResult>)validateMethod.Invoke(
                validator,
                [argument, cancellationToken])!;

            var result = await validationTask.ConfigureAwait(false);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        return await next().ConfigureAwait(false);
    }
}
