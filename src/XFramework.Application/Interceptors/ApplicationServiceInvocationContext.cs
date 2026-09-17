using System.Reflection;
using Castle.DynamicProxy;

namespace XFramework.Application.Interceptors;

public sealed class ApplicationServiceInvocationContext
{
    public ApplicationServiceInvocationContext(IInvocation invocation)
    {
        Invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));

        ImplementationType = invocation.InvocationTarget?.GetType()
            ?? invocation.TargetType
            ?? throw new InvalidOperationException("Unable to determine application service type.");

        Method = ResolveImplementationMethod(ImplementationType, invocation.Method);
        CancellationToken = ResolveCancellationToken(invocation.Arguments);
    }

    public IInvocation Invocation { get; }
    public Type ImplementationType { get; }
    public MethodInfo Method { get; }
    public object?[] Arguments => Invocation.Arguments;
    public CancellationToken CancellationToken { get; }
    public string ServiceName => ImplementationType.FullName ?? ImplementationType.Name;
    public string MethodName => Method.Name;

    public bool IsReadOnly =>
        Method.GetCustomAttribute<Attributes.ReadOnlyAttribute>(true) is not null;

    public Attributes.UnitOfWorkAttribute? UnitOfWorkAttribute =>
        Method.GetCustomAttribute<Attributes.UnitOfWorkAttribute>(true)
        ?? ImplementationType.GetCustomAttribute<Attributes.UnitOfWorkAttribute>(true);

    public bool RequiresValidation =>
        Method.GetCustomAttribute<Attributes.ValidateAttribute>(true) is not null
        || ImplementationType.GetCustomAttribute<Attributes.ValidateAttribute>(true) is not null;

    private static MethodInfo ResolveImplementationMethod(Type implementationType, MethodInfo invokedMethod)
    {
        var parameterTypes = invokedMethod.GetParameters()
            .Select(x => x.ParameterType)
            .ToArray();

        return implementationType.GetMethod(
                   invokedMethod.Name,
                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                   binder: null,
                   types: parameterTypes,
                   modifiers: null)
               ?? invokedMethod;
    }

    private static CancellationToken ResolveCancellationToken(object?[] arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument is CancellationToken cancellationToken)
                return cancellationToken;
        }

        return CancellationToken.None;
    }
}
