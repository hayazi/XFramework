using XFramework.Domain.Events;

namespace XFramework.Domain.Dimensions;

[EventType("XFramework.Dimensions.CustomDimensionCreated", 1, "dimensions.custom.created")]
public sealed record CustomDimensionCreated(
    Guid DimensionId,
    string Code,
    string Name,
    string DimensionKey) : DomainEvent;

[EventType("XFramework.Dimensions.CustomDimensionUpdated", 1, "dimensions.custom.updated")]
public sealed record CustomDimensionUpdated(
    Guid DimensionId,
    string Code,
    string Name,
    string DimensionKey) : DomainEvent;

[EventType("XFramework.Dimensions.CustomDimensionActivated", 1, "dimensions.custom.activated")]
public sealed record CustomDimensionActivated(
    Guid DimensionId) : DomainEvent;

[EventType("XFramework.Dimensions.CustomDimensionDeactivated", 1, "dimensions.custom.deactivated")]
public sealed record CustomDimensionDeactivated(
    Guid DimensionId) : DomainEvent;