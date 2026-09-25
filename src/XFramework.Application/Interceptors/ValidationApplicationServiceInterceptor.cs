using XFramework.Application.Exceptions;
using XFramework.Application.Validation;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Contracts.Security;
using Microsoft.Extensions.Logging;

namespace XFramework.Application.Interceptors;

public sealed class ValidationApplicationServiceInterceptor(
    IServiceProvider serviceProvider,
    ICurrentUser currentUser,
    ILogger<ValidationApplicationServiceInterceptor> logger)
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

            var method = validatorType.GetMethod(nameof(IValidator<object>.ValidateAsync))!;
            var task = (Task<ValidationResult>)method.Invoke(
                validator,
                [argument, cancellationToken])!;

            var result = await task.ConfigureAwait(false);

            if (!result.IsValid)
            {
                logger.LogValidationFailure(
                    currentUser.IsAuthenticated ? currentUser.UserId : null,
                    currentUser.IsAuthenticated ? currentUser.UserName : null,
                    context.MethodName,
                    result.Errors,
                    null);

                throw new ValidationException(result.Errors);
            }
        }

        return await next().ConfigureAwait(false);
    }
}
