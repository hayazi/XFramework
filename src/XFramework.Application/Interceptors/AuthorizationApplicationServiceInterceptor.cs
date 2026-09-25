using XFramework.Application.Attributes;
using XFramework.Application.Authorization;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Contracts.Security;
using XFramework.Application.Metadata;
using Microsoft.Extensions.Logging;

namespace XFramework.Application.Interceptors;

public sealed class AuthorizationApplicationServiceInterceptor(
    IPermissionChecker permissionChecker,
    ICurrentUser currentUser,
    ILogger<AuthorizationApplicationServiceInterceptor> logger)
    : IApplicationServiceInterceptor
{
    public async Task<object?> InterceptAsync(
        ApplicationServiceInvocationContext context,
        Func<Task<object?>> next,
        CancellationToken cancellationToken = default)
    {
        var permissions = GetRequiredPermissions(context);

        foreach (var permission in permissions)
        {
            try
            {
                await permissionChecker.CheckAsync(permission, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (AuthorizationException)
            {
                logger.LogPermissionDenied(
                    currentUser.IsAuthenticated ? currentUser.UserId : null,
                    currentUser.IsAuthenticated ? currentUser.UserName : null,
                    permission,
                    null);
                throw;
            }
        }

        return await next().ConfigureAwait(false);
    }

    private static IReadOnlyList<string> GetRequiredPermissions(
        ApplicationServiceInvocationContext context)
    {
        var permissions = new List<string>();

        permissions.AddRange(
            ApplicationMethodMetadata.GetAttributes<AuthorizeAttribute>(
                    context.ImplementationType,
                    context.Method)
                .Select(x => x.PermissionName));

        permissions.AddRange(
            ApplicationMethodMetadata.GetAttributes<AuthorizeAttribute>(
                    context.ImplementationType)
                .Select(x => x.PermissionName));

        return permissions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
