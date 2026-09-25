using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;
using Xunit;

namespace XFramework.Tests.Domain;

public class FiscalPeriodTests
{
    [Fact]
    public void Create_ValidData_CreatesFiscalPeriod()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);

        Assert.NotEqual(Guid.Empty, period.Id);
        Assert.Equal(2026, period.Year);
        Assert.Equal(1, period.PeriodNumber);
        Assert.Equal("FY2026", period.Name);
        Assert.Equal(dateRange, period.DateRange);
        Assert.Equal(FiscalPeriodStatus.Open, period.Status);
    }

    [Fact]
    public void Create_YearOutOfRange_Throws()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        Assert.Throws<ArgumentOutOfRangeException>(() => FiscalPeriod.Create(1899, 1, "FY2026", dateRange));
        Assert.Throws<ArgumentOutOfRangeException>(() => FiscalPeriod.Create(2101, 1, "FY2026", dateRange));
    }

    [Fact]
    public void Create_PeriodNumberOutOfRange_Throws()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        Assert.Throws<ArgumentOutOfRangeException>(() => FiscalPeriod.Create(2026, 0, "FY2026", dateRange));
        Assert.Throws<ArgumentOutOfRangeException>(() => FiscalPeriod.Create(2026, 14, "FY2026", dateRange));
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        Assert.Throws<ArgumentException>(() => FiscalPeriod.Create(2026, 1, "", dateRange));
    }

    [Fact]
    public void Close_OpenPeriod_ClosesPeriod()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);
        var closedBy = Guid.NewGuid();

        period.Close(closedBy);

        Assert.Equal(FiscalPeriodStatus.Closed, period.Status);
        Assert.NotNull(period.ClosedOnUtc);
        Assert.Equal(closedBy, period.ClosedBy);
    }

    [Fact]
    public void Close_AlreadyClosedPeriod_Throws()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);
        period.Close(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => period.Close(Guid.NewGuid()));
    }

    [Fact]
    public void Close_LockedPeriod_Throws()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);
        period.Lock();

        Assert.Throws<InvalidOperationException>(() => period.Close(Guid.NewGuid()));
    }

    [Fact]
    public void Reopen_ClosedPeriod_ReopensPeriod()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);
        period.Close(Guid.NewGuid());

        period.Reopen();

        Assert.Equal(FiscalPeriodStatus.Open, period.Status);
        Assert.Null(period.ClosedOnUtc);
        Assert.Null(period.ClosedBy);
    }

    [Fact]
    public void Reopen_OpenPeriod_Throws()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);

        Assert.Throws<InvalidOperationException>(() => period.Reopen());
    }

    [Fact]
    public void Lock_OpenPeriod_LocksPeriod()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);

        period.Lock();

        Assert.Equal(FiscalPeriodStatus.Locked, period.Status);
    }

    [Fact]
    public void Lock_AlreadyLockedPeriod_Throws()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);
        period.Lock();

        Assert.Throws<InvalidOperationException>(() => period.Lock());
    }

    [Fact]
    public void Unlock_LockedPeriod_UnlocksPeriod()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);
        period.Lock();

        period.Unlock();

        Assert.Equal(FiscalPeriodStatus.Open, period.Status);
    }

    [Fact]
    public void Unlock_OpenPeriod_Throws()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);

        Assert.Throws<InvalidOperationException>(() => period.Unlock());
    }

    [Fact]
    public void Contains_DateInRange_ReturnsTrue()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);

        Assert.True(period.Contains(new DateTime(2026, 6, 15)));
        Assert.True(period.Contains(new DateTime(2026, 1, 1)));
        Assert.True(period.Contains(new DateTime(2026, 12, 31)));
    }

    [Fact]
    public void Contains_DateOutOfRange_ReturnsFalse()
    {
        var dateRange = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var period = FiscalPeriod.Create(2026, 1, "FY2026", dateRange);

        Assert.False(period.Contains(new DateTime(2025, 12, 31)));
        Assert.False(period.Contains(new DateTime(2027, 1, 1)));
    }
}