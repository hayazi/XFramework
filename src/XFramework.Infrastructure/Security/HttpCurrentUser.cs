using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using XFramework.Application.Contracts.Authorization;

namespace XFramework.Infrastructure.Security;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;

    public string? UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        Principal?.Identity?.Name;

    public string? UserNameDisplay => UserName;

    public IReadOnlyCollection<string> Roles =>
        Principal?
            .FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? [];

    public bool IsInRole(string role) =>
        !string.IsNullOrWhiteSpace(role) &&
        Principal?.IsInRole(role) == true;
}
