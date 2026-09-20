using System.Reflection;

namespace XFramework.Application.Discovery;

internal sealed class ApplicationServiceDescriptor
{
    public Type ImplementationType { get; }

    public Type[] ServiceTypes { get; }

    public Assembly Assembly =>
        ImplementationType.Assembly;

    public ApplicationServiceDescriptor(
        Type implementationType,
        Type[] serviceTypes)
    {
        ImplementationType = implementationType;
        ServiceTypes = serviceTypes;
    }
}