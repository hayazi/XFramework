using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Authorization;

internal static class PermissionProviderDiscovery
{
    public static void Register(
        IServiceCollection services,
        params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies.Distinct())
        {
            foreach (var type in GetLoadableTypes(assembly))
            {
                if (!IsProvider(type))
                {
                    continue;
                }

                services.AddSingleton(
                    typeof(IPermissionDefinitionProvider),
                    type);
            }
        }
    }

    private static bool IsProvider(Type type)
    {
        return type is
        {
            IsClass: true,
            IsAbstract: false
        }
        &&
        typeof(IPermissionDefinitionProvider)
            .IsAssignableFrom(type);
    }

    private static IEnumerable<Type> GetLoadableTypes(
        Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types
                .Where(type => type is not null)!
                .Cast<Type>();
        }
    }
}