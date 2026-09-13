using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Pipelines;

public sealed class AuthorizationInterceptor(
    IPermissionChecker permissionChecker)
    : IApplicationServiceInterceptor
{
    public async Task InvokeAsync(
        ApplicationServiceInvocationContext context,
        Func<Task> next)
    {
        var attribute =
            context.Method
                .GetCustomAttributes(
                    typeof(Authorization.RequiresPermissionAttribute),
                    true)
                .Cast<Authorization.RequiresPermissionAttribute>()
                .FirstOrDefault();

        if (attribute is not null)
        {
            await permissionChecker.CheckAsync(
                attribute.Permission,
                context.CancellationToken);
        }

        await next();
    }
}