using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Inventory;

public sealed class KardexEntry : AggregateRoot<Guid>
{
    private KardexEntry() { }

    public Guid ItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public InventoryTransactionType TransactionType { get; private set; }
    public string DocumentReference { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public Quantity QuantityIn { get; private set; }
    public Quantity QuantityOut { get; private set; }
    public Quantity RunningBalance { get; private set; }
    public Money UnitCost { get; private set; }
    public Money TotalCost { get; private set; }
    public CostingMethod CostingMethod { get; private set; }
    public string? Description { get; private set; }
    public Guid? ReferenceDocumentId { get; private set; }
    public int? ReferenceDocumentLine { get; private set; }

    public static KardexEntry CreateReceipt(
        Guid itemId,
        Guid warehouseId,
        string documentReference,
        DateTime transactionDate,
        Quantity quantity,
        Money unitCost,
        CostingMethod costingMethod,
        string? description = null,
        Guid? referenceDocumentId = null,
        int? referenceDocumentLine = null)
    {
        if (quantity.Value <= 0)
            throw new ArgumentException("Receipt quantity must be positive.", nameof(quantity));
        if (unitCost.Amount < 0)
            throw new ArgumentException("Unit cost cannot be negative.", nameof(unitCost));

        var totalCost = unitCost * quantity.Value;
        var runningBalance = quantity;

        var entry = new KardexEntry
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            WarehouseId = warehouseId,
            TransactionType = InventoryTransactionType.Receipt,
            DocumentReference = documentReference.Trim(),
            TransactionDate = transactionDate,
            QuantityIn = quantity,
            QuantityOut = Quantity.Zero(quantity.Unit),
            RunningBalance = runningBalance,
            UnitCost = unitCost,
            TotalCost = totalCost,
            CostingMethod = costingMethod,
            Description = description?.Trim(),
            ReferenceDocumentId = referenceDocumentId,
            ReferenceDocumentLine = referenceDocumentLine
        };

        entry.AddDomainEvent(new KardexEntryCreated(entry.Id, entry.ItemId, entry.WarehouseId, entry.TransactionType, entry.RunningBalance));
        return entry;
    }

    public static KardexEntry CreateIssue(
        Guid itemId,
        Guid warehouseId,
        string documentReference,
        DateTime transactionDate,
        Quantity quantity,
        Money unitCost,
        Quantity runningBalanceBeforeIssue,
        CostingMethod costingMethod,
        string? description = null,
        Guid? referenceDocumentId = null,
        int? referenceDocumentLine = null)
    {
        if (quantity.Value <= 0)
            throw new ArgumentException("Issue quantity must be positive.", nameof(quantity));
        if (runningBalanceBeforeIssue.Value < quantity.Value)
            throw new InvalidOperationException("Insufficient stock for issue.");

        var totalCost = unitCost * quantity.Value;
        var runningBalance = new Quantity(runningBalanceBeforeIssue.Value - quantity.Value, quantity.Unit);

        var entry = new KardexEntry
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            WarehouseId = warehouseId,
            TransactionType = InventoryTransactionType.Issue,
            DocumentReference = documentReference.Trim(),
            TransactionDate = transactionDate,
            QuantityIn = Quantity.Zero(quantity.Unit),
            QuantityOut = quantity,
            RunningBalance = runningBalance,
            UnitCost = unitCost,
            TotalCost = totalCost,
            CostingMethod = costingMethod,
            Description = description?.Trim(),
            ReferenceDocumentId = referenceDocumentId,
            ReferenceDocumentLine = referenceDocumentLine
        };

        entry.AddDomainEvent(new KardexEntryCreated(entry.Id, entry.ItemId, entry.WarehouseId, entry.TransactionType, entry.RunningBalance));
        return entry;
    }

    public static KardexEntry CreateAdjustment(
        Guid itemId,
        Guid warehouseId,
        string documentReference,
        DateTime transactionDate,
        Quantity quantityIn,
        Quantity quantityOut,
        Quantity runningBalance,
        Money unitCost,
        CostingMethod costingMethod,
        string? description = null)
    {
        if (quantityIn.Value < 0 || quantityOut.Value < 0)
            throw new ArgumentException("Quantities cannot be negative.", nameof(quantityIn));

        var totalCost = unitCost * (quantityIn.Value - quantityOut.Value);

        var entry = new KardexEntry
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            WarehouseId = warehouseId,
            TransactionType = InventoryTransactionType.Adjustment,
            DocumentReference = documentReference.Trim(),
            TransactionDate = transactionDate,
            QuantityIn = quantityIn,
            QuantityOut = quantityOut,
            RunningBalance = runningBalance,
            UnitCost = unitCost,
            TotalCost = totalCost,
            CostingMethod = costingMethod,
            Description = description?.Trim()
        };

        entry.AddDomainEvent(new KardexEntryCreated(entry.Id, entry.ItemId, entry.WarehouseId, entry.TransactionType, entry.RunningBalance));
        return entry;
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }
}