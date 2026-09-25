using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Inventory;

public sealed class Item : AggregateRoot<Guid>
{
    private Item() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ItemType Type { get; private set; }
    public ItemStatus Status { get; private set; }
    public UnitOfMeasure BaseUnit { get; private set; }
    public CostingMethod CostingMethod { get; private set; }
    public Money StandardCost { get; private set; }
    public bool IsStocked { get; private set; }
    public bool IsPurchasable { get; private set; }
    public bool IsSellable { get; private set; }
    public bool IsProducible { get; private set; }
    public decimal? MinimumStock { get; private set; }
    public decimal? MaximumStock { get; private set; }
    public decimal? ReorderPoint { get; private set; }
    public Guid? DefaultWarehouseId { get; private set; }
    public string? Barcode { get; private set; }

    public static Item Create(
        string code,
        string name,
        ItemType type,
        UnitOfMeasure baseUnit,
        CostingMethod costingMethod,
        Currency currency,
        bool isStocked = true,
        bool isPurchasable = true,
        bool isSellable = true,
        bool isProducible = false,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Item code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name is required.", nameof(name));

        var item = new Item
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            BaseUnit = baseUnit,
            CostingMethod = costingMethod,
            StandardCost = Money.Zero(currency),
            Status = ItemStatus.Active,
            IsStocked = isStocked,
            IsPurchasable = isPurchasable,
            IsSellable = isSellable,
            IsProducible = isProducible
        };

        item.AddDomainEvent(new ItemCreated(item.Id, item.Code, item.Name, item.Type));
        return item;
    }

    public void UpdateDetails(
        string name,
        string? description,
        ItemStatus? status = null,
        CostingMethod? costingMethod = null,
        Money? standardCost = null,
        bool? isStocked = null,
        bool? isPurchasable = null,
        bool? isSellable = null,
        bool? isProducible = null,
        decimal? minimumStock = null,
        decimal? maximumStock = null,
        decimal? reorderPoint = null,
        Guid? defaultWarehouseId = null,
        string? barcode = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        
        if (status.HasValue) Status = status.Value;
        if (costingMethod.HasValue) CostingMethod = costingMethod.Value;
        if (standardCost.HasValue)
        {
            if (standardCost.Value.Currency != StandardCost.Currency)
                throw new InvalidOperationException("Standard cost currency must match item currency.");
            StandardCost = standardCost.Value;
        }
        if (isStocked.HasValue) IsStocked = isStocked.Value;
        if (isPurchasable.HasValue) IsPurchasable = isPurchasable.Value;
        if (isSellable.HasValue) IsSellable = isSellable.Value;
        if (isProducible.HasValue) IsProducible = isProducible.Value;
        if (minimumStock.HasValue) MinimumStock = minimumStock.Value;
        if (maximumStock.HasValue) MaximumStock = maximumStock.Value;
        if (reorderPoint.HasValue) ReorderPoint = reorderPoint.Value;
        if (defaultWarehouseId.HasValue) DefaultWarehouseId = defaultWarehouseId.Value;
        if (barcode != null) Barcode = barcode.Trim();

        AddDomainEvent(new ItemUpdated(Id, Code, Name));
    }

    public void Activate()
    {
        if (Status == ItemStatus.Active) return;
        if (Status == ItemStatus.Discontinued)
            throw new InvalidOperationException("Cannot activate a discontinued item.");
        
        Status = ItemStatus.Active;
        AddDomainEvent(new ItemActivated(Id));
    }

    public void Deactivate()
    {
        if (Status == ItemStatus.Inactive) return;
        
        Status = ItemStatus.Inactive;
        AddDomainEvent(new ItemDeactivated(Id));
    }

    public void Discontinue()
    {
        if (Status == ItemStatus.Discontinued) return;
        
        Status = ItemStatus.Discontinued;
        AddDomainEvent(new ItemDiscontinued(Id));
    }

    public void Block()
    {
        if (Status == ItemStatus.Blocked) return;
        
        Status = ItemStatus.Blocked;
        AddDomainEvent(new ItemBlocked(Id));
    }

    public void Unblock()
    {
        if (Status != ItemStatus.Blocked) return;
        
        Status = ItemStatus.Active;
        AddDomainEvent(new ItemUnblocked(Id));
    }
}