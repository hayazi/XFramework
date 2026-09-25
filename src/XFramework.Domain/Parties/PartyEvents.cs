using XFramework.Domain.Events;

namespace XFramework.Domain.Parties;

public sealed record PartyCreated(
    Guid PartyId,
    string Code,
    string Name) : DomainEvent
{
    public Guid PartyId { get; init; } = PartyId;
    public string Code { get; init; } = Code;
    public string Name { get; init; } = Name;
}

public sealed record PartyUpdated(
    Guid PartyId,
    string Name) : DomainEvent
{
    public Guid PartyId { get; init; } = PartyId;
    public string Name { get; init; } = Name;
}

public sealed record PartyRoleAssigned(
    Guid PartyId,
    PartyRole Role) : DomainEvent
{
    public Guid PartyId { get; init; } = PartyId;
    public PartyRole Role { get; init; } = Role;
}

public sealed record PartyRoleRemoved(
    Guid PartyId,
    PartyRole Role) : DomainEvent
{
    public Guid PartyId { get; init; } = PartyId;
    public PartyRole Role { get; init; } = Role;
}

public sealed record PartyActivated(
    Guid PartyId) : DomainEvent
{
    public Guid PartyId { get; init; } = PartyId;
}

public sealed record PartyDeactivated(
    Guid PartyId) : DomainEvent
{
    public Guid PartyId { get; init; } = PartyId;
}