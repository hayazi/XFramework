using XFramework.Domain.Events;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Inventory;

[EventType("XFramework.Inventory.KardexEntryCreated", 1, "inventory.kardex.created")]
public sealed record KardexEntryCreated(
    Guid KardexEntryId,
    Guid ItemId,
    Guid WarehouseId,
    InventoryTransactionType TransactionType,
    Quantity RunningBalance) : DomainEvent;