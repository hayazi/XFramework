using XFramework.Domain.SharedKernel;
using Xunit;

namespace XFramework.Tests.Domain;

public class SharedKernelValueObjectTests
{
    [Fact]
    public void Quantity_Creation_RoundsToFourDecimals()
    {
        var qty = new Quantity(123.456789m, UnitOfMeasure.Kilogram);
        Assert.Equal(123.4568m, qty.Value);
    }

    [Fact]
    public void Quantity_Addition_SameUnit_ReturnsSum()
    {
        var q1 = new Quantity(10m, UnitOfMeasure.Kilogram);
        var q2 = new Quantity(5m, UnitOfMeasure.Kilogram);
        var result = q1 + q2;
        Assert.Equal(15m, result.Value);
        Assert.Equal(UnitOfMeasure.Kilogram, result.Unit);
    }

    [Fact]
    public void Quantity_Addition_DifferentUnit_Throws()
    {
        var q1 = new Quantity(10m, UnitOfMeasure.Kilogram);
        var q2 = new Quantity(5m, UnitOfMeasure.Liter);
        Assert.Throws<InvalidOperationException>(() => q1 + q2);
    }

    [Fact]
    public void Quantity_Subtraction_SameUnit_ReturnsDifference()
    {
        var q1 = new Quantity(10m, UnitOfMeasure.Kilogram);
        var q2 = new Quantity(3m, UnitOfMeasure.Kilogram);
        var result = q1 - q2;
        Assert.Equal(7m, result.Value);
    }

    [Fact]
    public void Quantity_Multiplication_ByDecimal_ReturnsProduct()
    {
        var qty = new Quantity(10m, UnitOfMeasure.Kilogram);
        var result = qty * 2.5m;
        Assert.Equal(25m, result.Value);
    }

    [Fact]
    public void Quantity_Division_ByDecimal_ReturnsQuotient()
    {
        var qty = new Quantity(10m, UnitOfMeasure.Kilogram);
        var result = qty / 2m;
        Assert.Equal(5m, result.Value);
    }

    [Fact]
    public void Quantity_Division_ByZero_Throws()
    {
        var qty = new Quantity(10m, UnitOfMeasure.Kilogram);
        Assert.Throws<DivideByZeroException>(() => qty / 0m);
    }

    [Fact]
    public void Quantity_Equality_SameValues_ReturnsTrue()
    {
        var q1 = new Quantity(10m, UnitOfMeasure.Kilogram);
        var q2 = new Quantity(10m, UnitOfMeasure.Kilogram);
        Assert.True(q1 == q2);
        Assert.False(q1 != q2);
    }

    [Fact]
    public void Percentage_Creation_ValidatesRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Percentage(150m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Percentage(-10m));

        var p3 = new Percentage(50.5m);
        Assert.Equal(50.5m, p3.Value);
    }

    [Fact]
    public void Percentage_OfMoney_ReturnsMoney()
    {
        var pct = new Percentage(10m);
        var money = new Money(1000m, Currency.IRR);
        var result = pct * money;
        Assert.Equal(new Money(100m, Currency.IRR), result);
    }

    [Fact]
    public void Percentage_OfQuantity_ReturnsQuantity()
    {
        var pct = new Percentage(50m);
        var qty = new Quantity(100m, UnitOfMeasure.Kilogram);
        var result = pct * qty;
        Assert.Equal(new Quantity(50m, UnitOfMeasure.Kilogram), result);
    }

    [Fact]
    public void Percentage_Addition_ReturnsSum()
    {
        var p1 = new Percentage(30m);
        var p2 = new Percentage(20m);
        var result = p1 + p2;
        Assert.Equal(50m, result.Value);
    }

    [Fact]
    public void Percentage_Subtraction_ReturnsDifference()
    {
        var p1 = new Percentage(30m);
        var p2 = new Percentage(10m);
        var result = p1 - p2;
        Assert.Equal(20m, result.Value);
    }

    [Fact]
    public void Percentage_Multiplication_ByDecimal_ReturnsProduct()
    {
        var pct = new Percentage(50m);
        var result = pct * 2m;
        Assert.Equal(100m, result.Value);
    }

    [Fact]
    public void Percentage_Equality_SameValues_ReturnsTrue()
    {
        var p1 = new Percentage(50m);
        var p2 = new Percentage(50m);
        Assert.True(p1 == p2);
        Assert.False(p1 != p2);
    }

    [Fact]
    public void DateRange_EndBeforeStart_Throws()
    {
        var start = new DateTime(2026, 12, 31);
        var end = new DateTime(2026, 1, 1);
        Assert.Throws<ArgumentException>(() => new DateRange(start, end));
    }

    [Fact]
    public void DateRange_DurationDays_CalculatesCorrectly()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 10));
        Assert.Equal(10, range.DurationDays);
    }

    [Fact]
    public void DateRange_Contains_DateInRange_ReturnsTrue()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        Assert.True(range.Contains(new DateTime(2026, 6, 15)));
        Assert.True(range.Contains(new DateTime(2026, 1, 1)));
        Assert.True(range.Contains(new DateTime(2026, 12, 31)));
    }

    [Fact]
    public void DateRange_Contains_DateOutOfRange_ReturnsFalse()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        Assert.False(range.Contains(new DateTime(2025, 12, 31)));
        Assert.False(range.Contains(new DateTime(2027, 1, 1)));
    }

    [Fact]
    public void DateRange_Overlaps_OverlappingRanges_ReturnsTrue()
    {
        var range1 = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 6, 30));
        var range2 = new DateRange(new DateTime(2026, 4, 1), new DateTime(2026, 12, 31));
        Assert.True(range1.Overlaps(range2));
        Assert.True(range2.Overlaps(range1));
    }

    [Fact]
    public void DateRange_Overlaps_NonOverlappingRanges_ReturnsFalse()
    {
        var range1 = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 3, 31));
        var range2 = new DateRange(new DateTime(2026, 4, 1), new DateTime(2026, 6, 30));
        Assert.False(range1.Overlaps(range2));
    }

    [Fact]
    public void Address_Creation_TrimsValues()
    {
        var address = new Address(" 123 Main St ", " Tehran ", " Tehran ", " 12345 ", " Iran ");
        Assert.Equal("123 Main St", address.Street);
        Assert.Equal("Tehran", address.City);
        Assert.Equal("Tehran", address.State);
        Assert.Equal("12345", address.PostalCode);
        Assert.Equal("Iran", address.Country);
    }

    [Fact]
    public void Address_IsEmpty_AllEmpty_ReturnsTrue()
    {
        var address = new Address("", "", "", "");
        Assert.True(address.IsEmpty);
    }

    [Fact]
    public void Address_FullAddress_FormatsCorrectly()
    {
        var address = new Address("123 Main St", "Tehran", "Tehran", "12345", "Iran");
        Assert.Equal("123 Main St, Tehran, Tehran, 12345, Iran", address.FullAddress);
    }

    [Fact]
    public void Address_FullAddress_ExcludesEmptyValues()
    {
        var address = new Address("123 Main St", "Tehran", "", "12345", "");
        Assert.Equal("123 Main St, Tehran, 12345", address.FullAddress);
    }

    [Fact]
    public void Address_DefaultCountry_IsIran()
    {
        var address = new Address("123 Main St", "Tehran", "Tehran", "12345");
        Assert.Equal("Iran", address.Country);
    }

    [Fact]
    public void ContactInfo_Creation_TrimsValues()
    {
        var contact = new ContactInfo(" John Doe ", " +989123456789 ", " john@example.com ");
        Assert.Equal("John Doe", contact.Name);
        Assert.Equal("+989123456789", contact.Phone);
        Assert.Equal("john@example.com", contact.Email);
    }

    [Fact]
    public void ContactInfo_HasPhone_ReturnsTrueWhenPhoneExists()
    {
        var contact = new ContactInfo("John", "+989123456789");
        Assert.True(contact.HasPhone);
    }

    [Fact]
    public void ContactInfo_HasPhone_ReturnsFalseWhenPhoneEmpty()
    {
        var contact = new ContactInfo("John");
        Assert.False(contact.HasPhone);
    }

    [Fact]
    public void ContactInfo_HasEmail_ReturnsTrueWhenEmailExists()
    {
        var contact = new ContactInfo("John", email: "john@example.com");
        Assert.True(contact.HasEmail);
    }

    [Fact]
    public void ContactInfo_HasAddress_ReturnsTrueWhenAddressExists()
    {
        var address = new Address("123 Main St", "Tehran", "Tehran", "12345");
        var contact = new ContactInfo("John", address: address);
        Assert.True(contact.HasAddress);
    }

    [Fact]
    public void ContactInfo_HasAddress_ReturnsFalseWhenAddressEmpty()
    {
        var address = new Address("", "", "", "");
        var contact = new ContactInfo("John", address: address);
        Assert.False(contact.HasAddress);
    }

    [Fact]
    public void ContactInfo_ToString_FormatsCorrectly()
    {
        var contact = new ContactInfo("John Doe", "+989123456789", "john@example.com");
        Assert.Equal("John Doe | +989123456789 | john@example.com", contact.ToString());
    }
}