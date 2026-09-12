namespace XFramework.Application.Pipelines;

public interface IApplicationServiceInterceptor
{
    Task InvokeAsync(
        ApplicationServiceInvocationContext context,
        Func<Task> next);
}