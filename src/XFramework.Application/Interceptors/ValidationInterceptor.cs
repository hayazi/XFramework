using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Attributes;
using XFramework.Application.Metadata;
using XFramework.Application.Validation;

namespace XFramework.Application.Interceptors;

public sealed class ValidationInterceptor : IInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationInterceptor(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Intercept(IInvocation invocation)
    {
        if (!ShouldValidate(invocation))
        {
            invocation.Proceed();
            return;
        }

        ValidateArgumentsAsync(invocation)
            .GetAwaiter()
            .GetResult();

        invocation.Proceed();
    }

    private bool ShouldValidate(
        IInvocation invocation)
    {
        var implementationType =
            invocation.InvocationTarget.GetType();

        return
            ApplicationMethodMetadata
                .HasAttribute<ValidateAttribute>(
                    implementationType,
                    invocation.Method)
            ||
            ApplicationMethodMetadata
                .HasAttribute<ValidateAttribute>(
                    implementationType);
    }

    private async Task ValidateArgumentsAsync(
        IInvocation invocation)
    {
        foreach (var argument in invocation.Arguments)
        {
            if (argument is null)
                continue;

            var argumentType =
                argument.GetType();

            var validatorType =
                typeof(IValidator<>)
                    .MakeGenericType(argumentType);

            var validator =
                _serviceProvider
                    .GetService(validatorType);

            if (validator is null)
                continue;

            var method =
                validatorType.GetMethod(
                    nameof(
                        IValidator<object>
                            .ValidateAsync));

            if (method is null)
                continue;

            var resultTask =
                (Task<ValidationResult>)method.Invoke(
                    validator,
                    new object[]
                    {
                        argument,
                        CancellationToken.None
                    })!;

            var result =
                await resultTask;

            if (!result.IsValid)
            {
                throw new ValidationException(
                    result.Errors);
            }
        }
    }
}