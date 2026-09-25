using XFramework.Domain.Aggregates;

namespace XFramework.Domain.Dimensions;

public sealed class CustomDimension : AggregateRoot<Guid>
{
    private CustomDimension() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string DimensionKey { get; private set; } = string.Empty;
    public DimensionStatus Status { get; private set; }
    public bool IsRequired { get; private set; }
    public bool AllowHierarchy { get; private set; }
    public Guid? ParentDimensionId { get; private set; }
    public CustomDimension? ParentDimension { get; private set; }
    public List<CustomDimension> Children { get; private set; } = new();
    public Dictionary<string, string> Attributes { get; private set; } = new();

    public static CustomDimension Create(
        string code,
        string name,
        string dimensionKey,
        string? description = null,
        bool isRequired = false,
        bool allowHierarchy = false,
        Guid? parentDimensionId = null,
        Dictionary<string, string>? attributes = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Dimension code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Dimension name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(dimensionKey))
            throw new ArgumentException("Dimension key is required.", nameof(dimensionKey));

        var dimension = new CustomDimension
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            DimensionKey = dimensionKey.Trim().ToLowerInvariant(),
            Status = DimensionStatus.Active,
            IsRequired = isRequired,
            AllowHierarchy = allowHierarchy,
            ParentDimensionId = parentDimensionId,
            Attributes = attributes ?? new Dictionary<string, string>()
        };

        dimension.AddDomainEvent(new CustomDimensionCreated(dimension.Id, dimension.Code, dimension.Name, dimension.DimensionKey));
        return dimension;
    }

    public void UpdateDetails(
        string name,
        string? description,
        DimensionStatus? status = null,
        bool? isRequired = null,
        bool? allowHierarchy = null,
        Guid? parentDimensionId = null,
        Dictionary<string, string>? attributes = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        
        if (status.HasValue)
        {
            if (status.Value == DimensionStatus.Closed && Children.Any(c => c.Status == DimensionStatus.Active))
                throw new InvalidOperationException("Cannot close dimension with active children.");
            Status = status.Value;
        }
        
        if (isRequired.HasValue) IsRequired = isRequired.Value;
        if (allowHierarchy.HasValue) AllowHierarchy = allowHierarchy.Value;
        if (parentDimensionId.HasValue)
        {
            if (parentDimensionId.Value == Id)
                throw new InvalidOperationException("Dimension cannot be its own parent.");
            if (!AllowHierarchy)
                throw new InvalidOperationException("Hierarchy not allowed for this dimension.");
            ParentDimensionId = parentDimensionId.Value;
        }
        if (attributes != null) Attributes = attributes;

        AddDomainEvent(new CustomDimensionUpdated(Id, Code, Name, DimensionKey));
    }

    public void Activate()
    {
        if (Status == DimensionStatus.Active) return;
        Status = DimensionStatus.Active;
        AddDomainEvent(new CustomDimensionActivated(Id));
    }

    public void Deactivate()
    {
        if (Status == DimensionStatus.Inactive) return;
        if (Children.Any(c => c.Status == DimensionStatus.Active))
            throw new InvalidOperationException("Cannot deactivate dimension with active children.");
        
        Status = DimensionStatus.Inactive;
        AddDomainEvent(new CustomDimensionDeactivated(Id));
    }

    public void SetAttribute(string key, string value)
    {
        Attributes[key.Trim().ToLowerInvariant()] = value.Trim();
    }

    public void RemoveAttribute(string key)
    {
        Attributes.Remove(key.Trim().ToLowerInvariant());
    }
}