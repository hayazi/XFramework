using XFramework.Application.Abstractions;
namespace XFramework.Application.Pipelines;

public sealed class ValidationInterceptor(IValidator validator) : IApplicationServiceInterceptor
{
    public async Task InvokeAsync(ApplicationServiceInvocationContext context, Func<Task> next)
    {
        await validator.ValidateAsync(context.Arguments, context.CancellationToken);
        await next();
    }
}