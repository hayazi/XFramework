using XFramework.Domain.Events;

namespace XFramework.Domain.Inventory;

[EventType("XFramework.Inventory.WarehouseCreated", 1, "inventory.warehouse.created")]
public sealed record WarehouseCreated(
    Guid WarehouseId,
    string Code,
    string Name,
    WarehouseType Type) : DomainEvent;

[EventType("XFramework.Inventory.WarehouseUpdated", 1, "inventory.warehouse.updated")]
public sealed record WarehouseUpdated(
    Guid WarehouseId,
    string Code,
    string Name) : DomainEvent;

[EventType("XFramework.Inventory.WarehouseActivated", 1, "inventory.warehouse.activated")]
public sealed record WarehouseActivated(
    Guid WarehouseId) : DomainEvent;

[EventType("XFramework.Inventory.WarehouseDeactivated", 1, "inventory.warehouse.deactivated")]
public sealed record WarehouseDeactivated(
    Guid WarehouseId) : DomainEvent;