using XFramework.Domain.SharedKernel;
using Xunit;

namespace XFramework.Tests.Domain;

public class MoneyTests
{
    [Fact]
    public void Constructor_RoundsToTwoDecimalPlaces()
    {
        var money = new Money(123.456m, Currency.IRR);
        Assert.Equal(123.46m, money.Amount);
    }

    [Fact]
    public void Addition_SameCurrency_ReturnsSum()
    {
        var m1 = new Money(100m, Currency.IRR);
        var m2 = new Money(50m, Currency.IRR);
        var result = m1 + m2;
        Assert.Equal(150m, result.Amount);
        Assert.Equal(Currency.IRR, result.Currency);
    }

    [Fact]
    public void Addition_DifferentCurrency_Throws()
    {
        var m1 = new Money(100m, Currency.IRR);
        var m2 = new Money(50m, Currency.USD);
        Assert.Throws<InvalidOperationException>(() => m1 + m2);
    }

    [Fact]
    public void Subtraction_SameCurrency_ReturnsDifference()
    {
        var m1 = new Money(100m, Currency.IRR);
        var m2 = new Money(30m, Currency.IRR);
        var result = m1 - m2;
        Assert.Equal(70m, result.Amount);
    }

    [Fact]
    public void Subtraction_DifferentCurrency_Throws()
    {
        var m1 = new Money(100m, Currency.IRR);
        var m2 = new Money(30m, Currency.USD);
        Assert.Throws<InvalidOperationException>(() => m1 - m2);
    }

    [Fact]
    public void UnaryNegation_ReturnsNegatedAmount()
    {
        var money = new Money(100m, Currency.IRR);
        var result = -money;
        Assert.Equal(-100m, result.Amount);
        Assert.Equal(Currency.IRR, result.Currency);
    }

    [Fact]
    public void Multiplication_ByDecimal_ReturnsProduct()
    {
        var money = new Money(100m, Currency.IRR);
        var result = money * 1.5m;
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void Multiplication_DecimalByMoney_ReturnsProduct()
    {
        var money = new Money(100m, Currency.IRR);
        var result = 1.5m * money;
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void Division_ByDecimal_ReturnsQuotient()
    {
        var money = new Money(100m, Currency.IRR);
        var result = money / 2m;
        Assert.Equal(50m, result.Amount);
    }

    [Fact]
    public void Division_ByZero_Throws()
    {
        var money = new Money(100m, Currency.IRR);
        Assert.Throws<DivideByZeroException>(() => money / 0m);
    }

    [Fact]
    public void Comparison_GreaterThan_SameCurrency_ReturnsTrue()
    {
        var m1 = new Money(100m, Currency.IRR);
        var m2 = new Money(50m, Currency.IRR);
        Assert.True(m1 > m2);
        Assert.False(m1 < m2);
    }

    [Fact]
    public void Comparison_DifferentCurrency_Throws()
    {
        var m1 = new Money(100m, Currency.IRR);
        var m2 = new Money(50m, Currency.USD);
        Assert.Throws<InvalidOperationException>(() => _ = m1 > m2);
        Assert.Throws<InvalidOperationException>(() => _ = m1 < m2);
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        var m1 = new Money(100m, Currency.IRR);
        var m2 = new Money(100m, Currency.IRR);
        Assert.True(m1 == m2);
        Assert.False(m1 != m2);
        Assert.True(m1.Equals(m2));
    }

    [Fact]
    public void Zero_ReturnsZeroAmount()
    {
        var zero = Money.Zero(Currency.USD);
        Assert.Equal(0m, zero.Amount);
        Assert.Equal(Currency.USD, zero.Currency);
    }

[Fact]
    public void ToString_FormatsCorrectly()
    {
        var money = new Money(1234.5m, Currency.IRR);
        var result = money.ToString();
        // Format uses current culture (may use . or , as thousand/decimal separator)
        Assert.Contains("1", result);
        Assert.Contains("234", result);
        Assert.Contains("50", result);
        Assert.Contains("IRR", result);
    }
}