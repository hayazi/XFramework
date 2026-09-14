using System.Security.Claims;
using XFramework.Application.Abstractions;

namespace XFramework.EntityFrameworkCore.Security;

public sealed class HttpCurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    private ClaimsPrincipal? Principal =>
        httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;

    public string? UserId =>
        Principal?.FindFirstValue(
            ClaimTypes.NameIdentifier);

    public string? UserName =>
        Principal?.FindFirstValue(
            ClaimTypes.Name);

    public IReadOnlyCollection<string> Roles =>
        Principal?
            .FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Distinct()
            .ToArray()
        ?? [];

    public bool IsInRole(string role)
    {
        return Principal?.IsInRole(role) == true;
    }
}