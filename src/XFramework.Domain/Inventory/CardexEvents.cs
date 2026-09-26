using XFramework.Domain.Events;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Inventory;

[EventType("XFramework.Inventory.CardexEntryCreated", 1, "inventory.cardex.created")]
public sealed record CardexEntryCreated(
    Guid CardexEntryId,
    Guid ItemId,
    Guid WarehouseId,
    InventoryTransactionType TransactionType,
    Quantity RunningBalance) : DomainEvent;