namespace XFramework.Application.Interceptors;

public interface IApplicationServiceInterceptor
{
    Task<object?> InterceptAsync(
        ApplicationServiceInvocationContext context,
        Func<Task<object?>> next,
        CancellationToken cancellationToken = default);
}
