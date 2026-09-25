using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Dimensions;

public sealed class Project : AggregateRoot<Guid>
{
    private Project() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DimensionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public Money? Budget { get; private set; }
    public Guid? ManagerId { get; private set; }
    public Guid? CustomerId { get; private set; }

    public static Project Create(
        string code,
        string name,
        DateTime startDate,
        string? description = null,
        DateTime? endDate = null,
        Money? budget = null,
        Guid? managerId = null,
        Guid? customerId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Project code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Status = DimensionStatus.Active,
            StartDate = startDate,
            EndDate = endDate,
            Budget = budget,
            ManagerId = managerId,
            CustomerId = customerId
        };

        project.AddDomainEvent(new ProjectCreated(project.Id, project.Code, project.Name, project.StartDate));
        return project;
    }

    public void UpdateDetails(
        string name,
        string? description,
        DimensionStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        DateTime? actualEndDate = null,
        Money? budget = null,
        Guid? managerId = null,
        Guid? customerId = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        
        if (status.HasValue) Status = status.Value;
        if (startDate.HasValue) StartDate = startDate.Value;
        if (endDate.HasValue) EndDate = endDate.Value;
        if (actualEndDate.HasValue) ActualEndDate = actualEndDate.Value;
        if (budget.HasValue)
        {
            if (Budget.HasValue && budget.Value.Currency != Budget.Value.Currency)
                throw new InvalidOperationException("Budget currency cannot be changed.");
            Budget = budget.Value;
        }
        if (managerId.HasValue) ManagerId = managerId.Value;
        if (customerId.HasValue) CustomerId = customerId.Value;

        AddDomainEvent(new ProjectUpdated(Id, Code, Name));
    }

    public void Activate()
    {
        if (Status == DimensionStatus.Active) return;
        Status = DimensionStatus.Active;
        AddDomainEvent(new ProjectActivated(Id));
    }

    public void Deactivate()
    {
        if (Status == DimensionStatus.Inactive) return;
        Status = DimensionStatus.Inactive;
        AddDomainEvent(new ProjectDeactivated(Id));
    }

    public void Close(DateTime actualEndDate)
    {
        if (Status == DimensionStatus.Closed) return;
        
        Status = DimensionStatus.Closed;
        ActualEndDate = actualEndDate;
        AddDomainEvent(new ProjectClosed(Id, actualEndDate));
    }

    public bool IsActiveOn(DateTime date) => 
        Status == DimensionStatus.Active && 
        date >= StartDate && 
        (!EndDate.HasValue || date <= EndDate.Value);
}