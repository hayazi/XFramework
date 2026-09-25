using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Dimensions;

public sealed class CostCenter : AggregateRoot<Guid>
{
    private CostCenter() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DimensionStatus Status { get; private set; }
    public Guid? ParentCostCenterId { get; private set; }
    public CostCenter? ParentCostCenter { get; private set; }
    public List<CostCenter> Children { get; private set; } = new();
    public string? ManagerId { get; private set; }
    public decimal? BudgetAmount { get; private set; }
    public Currency? BudgetCurrency { get; private set; }

    public static CostCenter Create(
        string code,
        string name,
        string? description = null,
        Guid? parentCostCenterId = null,
        string? managerId = null,
        decimal? budgetAmount = null,
        Currency? budgetCurrency = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Cost center code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cost center name is required.", nameof(name));

        var costCenter = new CostCenter
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Status = DimensionStatus.Active,
            ParentCostCenterId = parentCostCenterId,
            ManagerId = managerId?.Trim(),
            BudgetAmount = budgetAmount,
            BudgetCurrency = budgetCurrency
        };

        costCenter.AddDomainEvent(new CostCenterCreated(costCenter.Id, costCenter.Code, costCenter.Name));
        return costCenter;
    }

    public void UpdateDetails(
        string name,
        string? description,
        DimensionStatus? status = null,
        Guid? parentCostCenterId = null,
        string? managerId = null,
        decimal? budgetAmount = null,
        Currency? budgetCurrency = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        
        if (status.HasValue)
        {
            if (status.Value == DimensionStatus.Closed && Children.Any(c => c.Status == DimensionStatus.Active))
                throw new InvalidOperationException("Cannot close cost center with active children.");
            Status = status.Value;
        }
        
        if (parentCostCenterId.HasValue)
        {
            if (parentCostCenterId.Value == Id)
                throw new InvalidOperationException("Cost center cannot be its own parent.");
            ParentCostCenterId = parentCostCenterId.Value;
        }
        
        if (managerId != null) ManagerId = managerId.Trim();
        if (budgetAmount.HasValue) BudgetAmount = budgetAmount.Value;
        if (budgetCurrency.HasValue) BudgetCurrency = budgetCurrency.Value;

        AddDomainEvent(new CostCenterUpdated(Id, Code, Name));
    }

    public void Activate()
    {
        if (Status == DimensionStatus.Active) return;
        Status = DimensionStatus.Active;
        AddDomainEvent(new CostCenterActivated(Id));
    }

    public void Deactivate()
    {
        if (Status == DimensionStatus.Inactive) return;
        if (Children.Any(c => c.Status == DimensionStatus.Active))
            throw new InvalidOperationException("Cannot deactivate cost center with active children.");
        
        Status = DimensionStatus.Inactive;
        AddDomainEvent(new CostCenterDeactivated(Id));
    }

    public void Close()
    {
        if (Status == DimensionStatus.Closed) return;
        if (Children.Any(c => c.Status == DimensionStatus.Active))
            throw new InvalidOperationException("Cannot close cost center with active children.");
        
        Status = DimensionStatus.Closed;
        AddDomainEvent(new CostCenterClosed(Id));
    }

    public void AddChild(CostCenter child)
    {
        if (Children.Any(c => c.Id == child.Id)) return;
        
        if (Status == DimensionStatus.Closed)
            throw new InvalidOperationException("Cannot add children to closed cost center.");
        
        Children.Add(child);
        child.ParentCostCenter = this;
    }
}