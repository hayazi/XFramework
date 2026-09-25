namespace XFramework.Domain.SharedKernel;

public readonly record struct DateRange : IEquatable<DateRange>
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public DateRange(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End date must be on or after start date.", nameof(end));
        Start = start.Date;
        End = end.Date;
    }

    public int DurationDays => (End - Start).Days + 1;

    public bool Contains(DateTime date) =>
        date.Date >= Start && date.Date <= End;

    public bool Overlaps(DateRange other) =>
        Start <= other.End && other.Start <= End;

    public override string ToString() =>
        $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd}";
}