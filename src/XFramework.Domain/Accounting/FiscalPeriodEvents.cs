using XFramework.Domain.Events;

namespace XFramework.Domain.Accounting;

public sealed record FiscalPeriodCreated(
    Guid PeriodId,
    int Year,
    int PeriodNumber,
    string Name) : DomainEvent
{
    public Guid PeriodId { get; init; } = PeriodId;
    public int Year { get; init; } = Year;
    public int PeriodNumber { get; init; } = PeriodNumber;
    public string Name { get; init; } = Name;
}

public sealed record FiscalPeriodClosed(
    Guid PeriodId,
    int Year,
    int PeriodNumber,
    Guid ClosedBy) : DomainEvent
{
    public Guid PeriodId { get; init; } = PeriodId;
    public int Year { get; init; } = Year;
    public int PeriodNumber { get; init; } = PeriodNumber;
    public Guid ClosedBy { get; init; } = ClosedBy;
}

public sealed record FiscalPeriodReopened(
    Guid PeriodId,
    int Year,
    int PeriodNumber) : DomainEvent
{
    public Guid PeriodId { get; init; } = PeriodId;
    public int Year { get; init; } = Year;
    public int PeriodNumber { get; init; } = PeriodNumber;
}

public sealed record FiscalPeriodLocked(
    Guid PeriodId,
    int Year,
    int PeriodNumber) : DomainEvent
{
    public Guid PeriodId { get; init; } = PeriodId;
    public int Year { get; init; } = Year;
    public int PeriodNumber { get; init; } = PeriodNumber;
}

public sealed record FiscalPeriodUnlocked(
    Guid PeriodId,
    int Year,
    int PeriodNumber) : DomainEvent
{
    public Guid PeriodId { get; init; } = PeriodId;
    public int Year { get; init; } = Year;
    public int PeriodNumber { get; init; } = PeriodNumber;
}