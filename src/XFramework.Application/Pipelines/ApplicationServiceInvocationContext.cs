namespace XFramework.Application.Pipelines;

public sealed class ApplicationServiceInvocationContext
{
    public object ServiceInstance { get; }

    public MethodInfo Method { get; }

    public object?[] Arguments { get; }

    public ApplicationServiceInvocationContext(
        object serviceInstance,
        MethodInfo method,
        object?[] arguments)
    {
        ServiceInstance = serviceInstance;
        Method = method;
        Arguments = arguments;
    }
}