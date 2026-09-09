using System.ComponentModel.DataAnnotations;

namespace XFramework.Application.Contracts.Identity;

public sealed class RoleCreateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(200)]
    public string? DisplayName { get; init; }

    public bool IsSystemRole { get; init; }
}