namespace XFramework.Application.Contracts.Authorization;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? UserName { get; }

    string? UserNameDisplay { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string role);
}