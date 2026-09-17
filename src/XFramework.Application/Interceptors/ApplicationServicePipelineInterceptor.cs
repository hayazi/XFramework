using Castle.DynamicProxy;

namespace XFramework.Application.Interceptors;

public sealed class ApplicationServicePipelineInterceptor
    : IInterceptor
{
    private readonly IReadOnlyList<IApplicationServiceInterceptor> _interceptors;

    public ApplicationServicePipelineInterceptor(
        IEnumerable<IApplicationServiceInterceptor> interceptors)
    {
        _interceptors = interceptors.ToArray();
    }

    public void Intercept(IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        var returnType = invocation.Method.ReturnType;

        if (returnType == typeof(Task))
        {
            invocation.ReturnValue = ExecuteTaskAsync(invocation);
            return;
        }

        if (returnType.IsGenericType
            && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = returnType.GetGenericArguments()[0];
            var method = typeof(ApplicationServicePipelineInterceptor)
                .GetMethod(nameof(ExecuteGenericTaskAsync),
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            invocation.ReturnValue = method
                .MakeGenericMethod(resultType)
                .Invoke(this, [invocation]);
            return;
        }

        invocation.Proceed();
    }

    private async Task ExecuteTaskAsync(IInvocation invocation)
    {
        await ExecutePipelineAsync(invocation).ConfigureAwait(false);
    }

    private async Task<T> ExecuteGenericTaskAsync<T>(IInvocation invocation)
    {
        var result = await ExecutePipelineAsync(invocation).ConfigureAwait(false);
        return result is null ? default! : (T)result;
    }

    private Task<object?> ExecutePipelineAsync(IInvocation invocation)
    {
        var context = new ApplicationServiceInvocationContext(invocation);
        return ExecuteAtAsync(context, 0);
    }

    private Task<object?> ExecuteAtAsync(
        ApplicationServiceInvocationContext context,
        int index)
    {
        if (index >= _interceptors.Count)
            return ProceedAsync(context.Invocation);

        var interceptor = _interceptors[index];
        return interceptor.InterceptAsync(
            context,
            () => ExecuteAtAsync(context, index + 1),
            context.CancellationToken);
    }

    private static async Task<object?> ProceedAsync(IInvocation invocation)
    {
        invocation.Proceed();
        var returnValue = invocation.ReturnValue;

        if (returnValue is Task task)
        {
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty?.GetValue(task);
        }

        return returnValue;
    }
}
