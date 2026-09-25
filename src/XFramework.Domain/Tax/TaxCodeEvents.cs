using XFramework.Domain.Events;

namespace XFramework.Domain.Tax;

[EventType("XFramework.Tax.TaxCodeCreated", 1, "tax.code.created")]
public sealed record TaxCodeCreated(
    Guid TaxCodeId,
    string Code,
    string Name,
    TaxType TaxType) : DomainEvent;

[EventType("XFramework.Tax.TaxCodeUpdated", 1, "tax.code.updated")]
public sealed record TaxCodeUpdated(
    Guid TaxCodeId,
    string Code,
    string Name) : DomainEvent;

[EventType("XFramework.Tax.TaxCodeActivated", 1, "tax.code.activated")]
public sealed record TaxCodeActivated(
    Guid TaxCodeId) : DomainEvent;

[EventType("XFramework.Tax.TaxCodeDeactivated", 1, "tax.code.deactivated")]
public sealed record TaxCodeDeactivated(
    Guid TaxCodeId) : DomainEvent;