namespace XFramework.Application.Pipelines;

public sealed class ApplicationServicePipeline
{
    private readonly IReadOnlyList<IApplicationServiceInterceptor> _interceptors;

    public ApplicationServicePipeline(
        IEnumerable<IApplicationServiceInterceptor> interceptors)
    {
        _interceptors = interceptors.ToList();
    }

    public Task ExecuteAsync(
        ApplicationServiceInvocationContext context,
        Func<Task> target)
    {
        Func<Task> pipeline = target;

        foreach (var interceptor in _interceptors.Reverse())
        {
            var next = pipeline;

            pipeline = () =>
                interceptor.InvokeAsync(context, next);
        }

        return pipeline();
    }
}