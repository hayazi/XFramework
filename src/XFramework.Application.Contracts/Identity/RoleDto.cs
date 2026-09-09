namespace XFramework.Application.Contracts.Identity;

public sealed class RoleDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? DisplayName { get; init; }

    public bool IsSystemRole { get; init; }

    public bool IsActive { get; init; }
}