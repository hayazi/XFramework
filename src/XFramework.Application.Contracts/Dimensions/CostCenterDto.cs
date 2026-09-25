using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Dimensions;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Dimensions;

public class CostCenterDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DimensionStatus Status { get; set; }
    public Guid? ParentCostCenterId { get; set; }
    public string? ParentCostCenterCode { get; set; }
    public string? ManagerId { get; set; }
    public decimal? BudgetAmount { get; set; }
    public Currency? BudgetCurrency { get; set; }
    public List<CostCenterDto> Children { get; set; } = new();
}