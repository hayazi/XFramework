using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Dimensions;

namespace XFramework.Application.Contracts.Dimensions;

public class CustomDimensionDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DimensionKey { get; set; } = string.Empty;
    public DimensionStatus Status { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowHierarchy { get; set; }
    public Guid? ParentDimensionId { get; set; }
    public string? ParentDimensionCode { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
    public List<CustomDimensionDto> Children { get; set; } = new();
}