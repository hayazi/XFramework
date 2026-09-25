using XFramework.Domain.Events;

namespace XFramework.Domain.Dimensions;

[EventType("XFramework.Dimensions.CostCenterCreated", 1, "dimensions.costcenter.created")]
public sealed record CostCenterCreated(
    Guid CostCenterId,
    string Code,
    string Name) : DomainEvent;

[EventType("XFramework.Dimensions.CostCenterUpdated", 1, "dimensions.costcenter.updated")]
public sealed record CostCenterUpdated(
    Guid CostCenterId,
    string Code,
    string Name) : DomainEvent;

[EventType("XFramework.Dimensions.CostCenterActivated", 1, "dimensions.costcenter.activated")]
public sealed record CostCenterActivated(
    Guid CostCenterId) : DomainEvent;

[EventType("XFramework.Dimensions.CostCenterDeactivated", 1, "dimensions.costcenter.deactivated")]
public sealed record CostCenterDeactivated(
    Guid CostCenterId) : DomainEvent;

[EventType("XFramework.Dimensions.CostCenterClosed", 1, "dimensions.costcenter.closed")]
public sealed record CostCenterClosed(
    Guid CostCenterId) : DomainEvent;