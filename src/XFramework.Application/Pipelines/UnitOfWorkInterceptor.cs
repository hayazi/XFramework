using XFramework.Application.Abstractions;
namespace XFramework.Application.Pipelines;

public sealed class UnitOfWorkInterceptor(IUnitOfWorkManager manager) : IApplicationServiceInterceptor
{
    public async Task InvokeAsync(ApplicationServiceInvocationContext context, Func<Task> next)
    {
        await using var scope = await manager.BeginAsync(context.CancellationToken);
        await next();
        await scope.CommitAsync(context.CancellationToken);
    }
}