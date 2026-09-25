using XFramework.Domain.Events;

namespace XFramework.Domain.Accounting;

public sealed record AccountCreated(
    Guid AccountId,
    string Code,
    string Name,
    AccountType Type) : DomainEvent
{
    public Guid AccountId { get; init; } = AccountId;
    public string Code { get; init; } = Code;
    public string Name { get; init; } = Name;
    public AccountType Type { get; init; } = Type;
}

public sealed record AccountUpdated(
    Guid AccountId,
    string Name) : DomainEvent
{
    public Guid AccountId { get; init; } = AccountId;
    public string Name { get; init; } = Name;
}

public sealed record AccountActivated(
    Guid AccountId) : DomainEvent
{
    public Guid AccountId { get; init; } = AccountId;
}

public sealed record AccountDeactivated(
    Guid AccountId) : DomainEvent
{
    public Guid AccountId { get; init; } = AccountId;
}

public sealed record JournalEntryCreated(
    Guid EntryId,
    string Reference,
    DateTime Date) : DomainEvent
{
    public Guid EntryId { get; init; } = EntryId;
    public string Reference { get; init; } = Reference;
    public DateTime Date { get; init; } = Date;
}

public sealed record JournalEntrySubmitted(
    Guid EntryId,
    string Reference) : DomainEvent
{
    public Guid EntryId { get; init; } = EntryId;
    public string Reference { get; init; } = Reference;
}

public sealed record JournalEntryApproved(
    Guid EntryId,
    Guid ApprovedBy) : DomainEvent
{
    public Guid EntryId { get; init; } = EntryId;
    public Guid ApprovedBy { get; init; } = ApprovedBy;
}

public sealed record JournalEntryRejected(
    Guid EntryId,
    string Reason) : DomainEvent
{
    public Guid EntryId { get; init; } = EntryId;
    public string Reason { get; init; } = Reason;
}

public sealed record JournalEntryPosted(
    Guid EntryId,
    string Reference,
    Guid PostedBy,
    DateTime PostedOnUtc) : DomainEvent
{
    public Guid EntryId { get; init; } = EntryId;
    public string Reference { get; init; } = Reference;
    public Guid PostedBy { get; init; } = PostedBy;
    public DateTime PostedOnUtc { get; init; } = PostedOnUtc;
}

public sealed record JournalEntryReversed(
    Guid EntryId,
    Guid ReversedBy,
    string Reason) : DomainEvent
{
    public Guid EntryId { get; init; } = EntryId;
    public Guid ReversedBy { get; init; } = ReversedBy;
    public string Reason { get; init; } = Reason;
}

public sealed record JournalEntryCancelled(
    Guid EntryId) : DomainEvent
{
    public Guid EntryId { get; init; } = EntryId;
}