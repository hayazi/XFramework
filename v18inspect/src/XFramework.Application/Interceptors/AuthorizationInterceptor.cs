using Castle.DynamicProxy;
using XFramework.Application.Attributes;
using XFramework.Application.Authorization;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Metadata;

namespace XFramework.Application.Interceptors;

public sealed class AuthorizationInterceptor : IInterceptor
{
    private readonly IPermissionChecker _permissionChecker;

    public AuthorizationInterceptor(
        IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public void Intercept(IInvocation invocation)
    {
        var permissions =
            GetRequiredPermissions(invocation);

        if (permissions.Count == 0)
        {
            invocation.Proceed();
            return;
        }

        CheckPermissionsAsync(permissions)
            .GetAwaiter()
            .GetResult();

        invocation.Proceed();
    }

    private async Task CheckPermissionsAsync(
        IReadOnlyList<string> permissions)
    {
        foreach (var permission in permissions)
        {
            var granted =
                await _permissionChecker.IsGrantedAsync(
                    permission);

            if (!granted)
            {
                throw new Exceptions.ForbiddenException(
                    permission);
            }
        }
    }

    private static IReadOnlyList<string>
        GetRequiredPermissions(
            IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        var target = invocation.InvocationTarget
            ?? throw new InvalidOperationException(
                "Unable to determine the invocation target.");

        var implementationType =
            invocation.TargetType ?? target.GetType();

        var method =
            invocation.Method;

        var permissions =
            new List<string>();

        permissions.AddRange(
            ApplicationMethodMetadata
                .GetAttributes<AuthorizeAttribute>(
                    implementationType,
                    method)
                .Select(x => x.PermissionName));

        permissions.AddRange(
            ApplicationMethodMetadata
                .GetAttributes<AuthorizeAttribute>(
                    implementationType)
                .Select(x => x.PermissionName));

        return permissions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}