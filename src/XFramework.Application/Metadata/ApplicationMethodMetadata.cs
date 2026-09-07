using System.Reflection;

namespace XFramework.Application.Metadata;

internal static class ApplicationMethodMetadata
{
    public static bool HasAttribute<TAttribute>(
        MethodInfo method)
        where TAttribute : Attribute
    {
        return method.IsDefined(
            typeof(TAttribute),
            inherit: true);
    }

    public static bool HasAttribute<TAttribute>(
        Type implementationType,
        MethodInfo interfaceMethod)
        where TAttribute : Attribute
    {
        if (HasAttribute<TAttribute>(interfaceMethod))
        {
            return true;
        }

        var implementationMethod =
            implementationType.GetMethod(
                interfaceMethod.Name,
                interfaceMethod
                    .GetParameters()
                    .Select(x => x.ParameterType)
                    .ToArray());

        return implementationMethod is not null
            && HasAttribute<TAttribute>(
                implementationMethod);
    }

    public static IReadOnlyList<TAttribute>
        GetAttributes<TAttribute>(
            Type implementationType,
            MethodInfo interfaceMethod)
        where TAttribute : Attribute
    {
        var attributes = new List<TAttribute>();

        attributes.AddRange(
            interfaceMethod
                .GetCustomAttributes<TAttribute>(
                    inherit: true));

        var implementationMethod =
            implementationType.GetMethod(
                interfaceMethod.Name,
                interfaceMethod
                    .GetParameters()
                    .Select(x => x.ParameterType)
                    .ToArray());

        if (implementationMethod is not null)
        {
            attributes.AddRange(
                implementationMethod
                    .GetCustomAttributes<TAttribute>(
                        inherit: true));
        }

        return attributes;
    }

    public static IReadOnlyList<TAttribute>
        GetAttributes<TAttribute>(
            Type implementationType)
        where TAttribute : Attribute
    {
        return implementationType
            .GetCustomAttributes<TAttribute>(
                inherit: true)
            .ToArray();
    }
}