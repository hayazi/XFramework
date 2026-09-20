using System.ComponentModel.DataAnnotations;

namespace XFramework.Application.Contracts.Identity;

public sealed class RoleUpdateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(200)]
    public string? DisplayName { get; init; }

    public bool IsActive { get; init; }
}