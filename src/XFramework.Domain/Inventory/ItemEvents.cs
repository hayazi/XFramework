using XFramework.Domain.Events;

namespace XFramework.Domain.Inventory;

[EventType("XFramework.Inventory.ItemCreated", 1, "inventory.item.created")]
public sealed record ItemCreated(
    Guid ItemId,
    string Code,
    string Name,
    ItemType Type) : DomainEvent;

[EventType("XFramework.Inventory.ItemUpdated", 1, "inventory.item.updated")]
public sealed record ItemUpdated(
    Guid ItemId,
    string Code,
    string Name) : DomainEvent;

[EventType("XFramework.Inventory.ItemActivated", 1, "inventory.item.activated")]
public sealed record ItemActivated(
    Guid ItemId) : DomainEvent;

[EventType("XFramework.Inventory.ItemDeactivated", 1, "inventory.item.deactivated")]
public sealed record ItemDeactivated(
    Guid ItemId) : DomainEvent;

[EventType("XFramework.Inventory.ItemDiscontinued", 1, "inventory.item.discontinued")]
public sealed record ItemDiscontinued(
    Guid ItemId) : DomainEvent;

[EventType("XFramework.Inventory.ItemBlocked", 1, "inventory.item.blocked")]
public sealed record ItemBlocked(
    Guid ItemId) : DomainEvent;

[EventType("XFramework.Inventory.ItemUnblocked", 1, "inventory.item.unblocked")]
public sealed record ItemUnblocked(
    Guid ItemId) : DomainEvent;