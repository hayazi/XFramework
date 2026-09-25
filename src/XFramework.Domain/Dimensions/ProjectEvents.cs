using XFramework.Domain.Events;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Dimensions;

[EventType("XFramework.Dimensions.ProjectCreated", 1, "dimensions.project.created")]
public sealed record ProjectCreated(
    Guid ProjectId,
    string Code,
    string Name,
    DateTime StartDate) : DomainEvent;

[EventType("XFramework.Dimensions.ProjectUpdated", 1, "dimensions.project.updated")]
public sealed record ProjectUpdated(
    Guid ProjectId,
    string Code,
    string Name) : DomainEvent;

[EventType("XFramework.Dimensions.ProjectActivated", 1, "dimensions.project.activated")]
public sealed record ProjectActivated(
    Guid ProjectId) : DomainEvent;

[EventType("XFramework.Dimensions.ProjectDeactivated", 1, "dimensions.project.deactivated")]
public sealed record ProjectDeactivated(
    Guid ProjectId) : DomainEvent;

[EventType("XFramework.Dimensions.ProjectClosed", 1, "dimensions.project.closed")]
public sealed record ProjectClosed(
    Guid ProjectId,
    DateTime ActualEndDate) : DomainEvent;