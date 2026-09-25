using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Inventory;

public sealed class Warehouse : AggregateRoot<Guid>
{
    private Warehouse() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public WarehouseType Type { get; private set; }
    public Address Address { get; private set; }
    public bool IsActive { get; private set; }
    public bool AllowNegativeStock { get; private set; }
    public Guid? ManagerId { get; private set; }

    public static Warehouse Create(
        string code,
        string name,
        WarehouseType type,
        Address address,
        bool allowNegativeStock = false,
        Guid? managerId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Warehouse code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name is required.", nameof(name));

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            Address = address,
            IsActive = true,
            AllowNegativeStock = allowNegativeStock,
            ManagerId = managerId
        };

        warehouse.AddDomainEvent(new WarehouseCreated(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.Type));
        return warehouse;
    }

    public void UpdateDetails(
        string name,
        string? description,
        WarehouseType? type = null,
        Address? address = null,
        bool? allowNegativeStock = null,
        Guid? managerId = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        
        if (type.HasValue) Type = type.Value;
        if (address.HasValue) Address = address.Value;
        if (allowNegativeStock.HasValue) AllowNegativeStock = allowNegativeStock.Value;
        if (managerId.HasValue) ManagerId = managerId.Value;

        AddDomainEvent(new WarehouseUpdated(Id, Code, Name));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        AddDomainEvent(new WarehouseActivated(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new WarehouseDeactivated(Id));
    }
}