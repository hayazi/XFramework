namespace XFramework.Application.Contracts.Identity;

public sealed class RolePermissionDto
{
    public Guid PermissionId { get; init; }

    public string PermissionName { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public bool IsGranted { get; init; }
}