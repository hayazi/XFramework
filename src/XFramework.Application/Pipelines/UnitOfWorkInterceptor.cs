using XFramework.Application.Abstractions;

namespace XFramework.Application.Pipelines;

public sealed class UnitOfWorkInterceptor(
    IUnitOfWork unitOfWork)
    : IApplicationServiceInterceptor
{
    public async Task InvokeAsync(
        ApplicationServiceInvocationContext context,
        Func<Task> next)
    {
        await using var transaction =
            await unitOfWork.BeginAsync(
                context.CancellationToken);

        try
        {
            await next();

            await transaction.SaveChangesAsync(
                context.CancellationToken);

            await transaction.CommitAsync(
                context.CancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                context.CancellationToken);

            throw;
        }
    }
}