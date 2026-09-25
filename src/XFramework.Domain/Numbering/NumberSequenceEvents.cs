using XFramework.Domain.Events;

namespace XFramework.Domain.Numbering;

[EventType("XFramework.Numbering.NumberSequenceCreated", 1, "numbering.sequence.created")]
public sealed record NumberSequenceCreated(
    Guid SequenceId,
    string Code,
    string Name) : DomainEvent;

[EventType("XFramework.Numbering.NumberSequenceUpdated", 1, "numbering.sequence.updated")]
public sealed record NumberSequenceUpdated(
    Guid SequenceId,
    string Code,
    string Name) : DomainEvent;

[EventType("XFramework.Numbering.NumberSequenceActivated", 1, "numbering.sequence.activated")]
public sealed record NumberSequenceActivated(
    Guid SequenceId) : DomainEvent;

[EventType("XFramework.Numbering.NumberSequenceDeactivated", 1, "numbering.sequence.deactivated")]
public sealed record NumberSequenceDeactivated(
    Guid SequenceId) : DomainEvent;

[EventType("XFramework.Numbering.NumberSequenceNumberGenerated", 1, "numbering.sequence.generated")]
public sealed record NumberSequenceNumberGenerated(
    Guid SequenceId,
    string Code,
    string GeneratedNumber,
    int CurrentNumber) : DomainEvent;

[EventType("XFramework.Numbering.NumberSequenceReset", 1, "numbering.sequence.reset")]
public sealed record NumberSequenceReset(
    Guid SequenceId,
    string Code) : DomainEvent;

[EventType("XFramework.Numbering.NumberSequenceExhausted", 1, "numbering.sequence.exhausted")]
public sealed record NumberSequenceExhausted(
    Guid SequenceId,
    string Code) : DomainEvent;