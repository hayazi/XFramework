using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Contracts.Security;

namespace XFramework.Infrastructure.Authorization;

public sealed class PermissionClaimsTransformation : IClaimsTransformation
{
    private readonly IPermissionChecker _permissionChecker;
    private readonly ICurrentUser _currentUser;

    public PermissionClaimsTransformation(
        IPermissionChecker permissionChecker,
        ICurrentUser currentUser)
    {
        _permissionChecker = permissionChecker;
        _currentUser = currentUser;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return principal;
        }

        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return principal;
        }

        if (identity.HasClaim(c => c.Type == "permission_loaded"))
        {
            return principal;
        }

        var permissions = await _permissionChecker.GetPermissionsAsync();
        
        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim("permission", permission));
        }

        identity.AddClaim(new Claim("permission_loaded", "true"));

        return principal;
    }
}