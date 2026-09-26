using Microsoft.AspNetCore.Authorization;
using XFramework.Application.Contracts.Authorization;

namespace XFramework.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionChecker _permissionChecker;

    public PermissionAuthorizationHandler(IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        var hasPermission = await _permissionChecker.IsGrantedAsync(requirement.PermissionName);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}