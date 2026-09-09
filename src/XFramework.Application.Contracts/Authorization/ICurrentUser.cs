namespace XFramework.Application.Contracts.Authorization;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    string? UserName { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string roleName);
}