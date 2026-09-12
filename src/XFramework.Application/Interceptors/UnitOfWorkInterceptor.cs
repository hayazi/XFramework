using System.Reflection;
using Castle.DynamicProxy;
using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Metadata;


namespace XFramework.Application.Interceptors;
public sealed class UnitOfWorkInterceptor
    : IApplicationServiceInterceptor
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public UnitOfWorkInterceptor(
        IUnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task InvokeAsync(
        ApplicationServiceInvocationContext context,
        Func<Task> next)
    {
        await using var uow =
            await _unitOfWorkManager.BeginAsync();

        await next();

        await uow.CommitAsync();
    }
}
/*
public sealed class UnitOfWorkInterceptor : IInterceptor
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkInterceptor(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void Intercept(IInvocation invocation)
    {
        if (!ShouldUseUnitOfWork(invocation))
        {
            invocation.Proceed();
            return;
        }

        invocation.Proceed();

        if (invocation.ReturnValue is Task task)
        {
            invocation.ReturnValue =
                ExecuteAsync(task);
        }
    }

    private bool ShouldUseUnitOfWork(
        IInvocation invocation)
    {
        var implementationType =
            invocation.InvocationTarget.GetType();

        var method =
            invocation.Method;

        return
            ApplicationMethodMetadata
                .HasAttribute<UnitOfWorkAttribute>(
                    implementationType,
                    method)
            ||
            ApplicationMethodMetadata
                .HasAttribute<UnitOfWorkAttribute>(
                    implementationType);
    }

    private async Task ExecuteAsync(Task task)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            await task;

            await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}
*/