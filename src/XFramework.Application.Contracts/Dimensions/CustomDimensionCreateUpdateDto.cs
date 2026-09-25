using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Dimensions;

namespace XFramework.Application.Contracts.Dimensions;

public class CustomDimensionCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DimensionKey { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public bool AllowHierarchy { get; set; } = false;
    public Guid? ParentDimensionId { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}

public class CustomDimensionUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DimensionStatus? Status { get; set; }
    public bool? IsRequired { get; set; }
    public bool? AllowHierarchy { get; set; }
    public Guid? ParentDimensionId { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}