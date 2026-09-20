using System.Reflection;
using XFramework.Application.Contracts.Abstractions;

namespace XFramework.Application.Discovery;

internal static class ApplicationServiceDiscovery
{
    public static IReadOnlyList<ApplicationServiceDescriptor>
        Discover(params System.Reflection.Assembly[] assemblies)
    {
        var result = new List<ApplicationServiceDescriptor>();

        foreach (var assembly in assemblies.Distinct())
        {
            var types = GetLoadableTypes(assembly);

            foreach (var implementationType in types)
            {
                if (!IsApplicationService(implementationType))
                {
                    continue;
                }

                var serviceTypes = implementationType
                    .GetInterfaces()
                    .Where(IsApplicationServiceInterface)
                    .ToArray();

                if (serviceTypes.Length == 0)
                {
                    continue;
                }

                result.Add(
                    new ApplicationServiceDescriptor(
                        implementationType,
                        serviceTypes));
            }
        }

        return result;
    }

    private static bool IsApplicationService(
        Type type)
    {
        return type is
        {
            IsClass: true,
            IsAbstract: false
        }
        &&
        type.Name.EndsWith(
            "AppService",
            StringComparison.Ordinal);
    }

    private static bool IsApplicationServiceInterface(
        Type type)
    {
        return typeof(IApplicationService)
            .IsAssignableFrom(type)
            && type != typeof(IApplicationService);
    }

    private static IEnumerable<Type> GetLoadableTypes(
        System.Reflection.Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types
                .Where(x => x is not null)!
                .Cast<Type>();
        }
    }
}