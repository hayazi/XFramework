using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Accounting;

public sealed class FiscalPeriod : AggregateRoot<Guid>
{
    private FiscalPeriod() { }

    public int Year { get; private set; }
    public int PeriodNumber { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateRange DateRange { get; private set; }
    public FiscalPeriodStatus Status { get; private set; }
    public DateTime? ClosedOnUtc { get; private set; }
    public Guid? ClosedBy { get; private set; }

    public static FiscalPeriod Create(
        int year,
        int periodNumber,
        string name,
        DateRange dateRange)
    {
        if (year < 1900 || year > 2100)
            throw new ArgumentOutOfRangeException(nameof(year));
        if (periodNumber < 1 || periodNumber > 13)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period must be between 1 and 13.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Period name is required.", nameof(name));

        var period = new FiscalPeriod
        {
            Id = Guid.NewGuid(),
            Year = year,
            PeriodNumber = periodNumber,
            Name = name.Trim(),
            DateRange = dateRange,
            Status = FiscalPeriodStatus.Open
        };

        period.AddDomainEvent(new FiscalPeriodCreated(period.Id, period.Year, period.PeriodNumber, period.Name));
        return period;
    }

    public void Close(Guid closedBy)
    {
        if (Status == FiscalPeriodStatus.Closed)
            throw new InvalidOperationException("Period is already closed.");
        if (Status == FiscalPeriodStatus.Locked)
            throw new InvalidOperationException("Cannot close a locked period. Unlock first.");

        Status = FiscalPeriodStatus.Closed;
        ClosedOnUtc = DateTime.UtcNow;
        ClosedBy = closedBy;

        AddDomainEvent(new FiscalPeriodClosed(Id, Year, PeriodNumber, closedBy));
    }

    public void Reopen()
    {
        if (Status != FiscalPeriodStatus.Closed)
            throw new InvalidOperationException("Only closed periods can be reopened.");

        Status = FiscalPeriodStatus.Open;
        ClosedOnUtc = null;
        ClosedBy = null;

        AddDomainEvent(new FiscalPeriodReopened(Id, Year, PeriodNumber));
    }

    public void Lock()
    {
        if (Status == FiscalPeriodStatus.Locked)
            throw new InvalidOperationException("Period is already locked.");

        Status = FiscalPeriodStatus.Locked;

        AddDomainEvent(new FiscalPeriodLocked(Id, Year, PeriodNumber));
    }

    public void Unlock()
    {
        if (Status != FiscalPeriodStatus.Locked)
            throw new InvalidOperationException("Only locked periods can be unlocked.");

        Status = FiscalPeriodStatus.Open;

        AddDomainEvent(new FiscalPeriodUnlocked(Id, Year, PeriodNumber));
    }

    public bool Contains(DateTime date) =>
        DateRange.Contains(date);
}

public enum FiscalPeriodStatus
{
    Open = 1,
    Closed = 2,
    Locked = 3
}