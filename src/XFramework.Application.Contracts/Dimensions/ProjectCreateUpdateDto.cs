using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Dimensions;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Dimensions;

public class ProjectCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public MoneyDto? Budget { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class ProjectUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DimensionStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public MoneyDto? Budget { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? CustomerId { get; set; }
}