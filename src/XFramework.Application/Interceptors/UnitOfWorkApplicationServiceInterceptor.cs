using XFramework.Application.Abstractions;

namespace XFramework.Application.Interceptors;

public sealed class UnitOfWorkApplicationServiceInterceptor(
    IUnitOfWork unitOfWork)
    : IApplicationServiceInterceptor
{
    public async Task<object?> InterceptAsync(
        ApplicationServiceInvocationContext context,
        Func<Task<object?>> next,
        CancellationToken cancellationToken = default)
    {
        if (!ShouldUseUnitOfWork(context))
            return await next().ConfigureAwait(false);

        await using var transaction = await unitOfWork
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var result = await next().ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken)
                .ConfigureAwait(false);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken)
                .ConfigureAwait(false);
            throw;
        }
    }

    private static bool ShouldUseUnitOfWork(
        ApplicationServiceInvocationContext context)
    {
        if (context.IsReadOnly)
            return false;

        if (context.UnitOfWorkAttribute is { } attribute)
            return attribute.IsTransactional;

        return IsMutationMethod(context.MethodName);
    }

    private static bool IsMutationMethod(string methodName)
    {
        return methodName.EndsWith("CreateAsync", StringComparison.Ordinal)
            || methodName.EndsWith("UpdateAsync", StringComparison.Ordinal)
            || methodName.EndsWith("DeleteAsync", StringComparison.Ordinal)
            || methodName.EndsWith("SaveAsync", StringComparison.Ordinal)
            || methodName.EndsWith("PostAsync", StringComparison.Ordinal)
            || methodName.EndsWith("ApplyAsync", StringComparison.Ordinal);
    }
}
