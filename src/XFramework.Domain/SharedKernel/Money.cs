using XFramework.Domain.Abstractions;

namespace XFramework.Domain.SharedKernel;

public readonly record struct Money : IEquatable<Money>
{
    public decimal Amount { get; init; }
    public Currency Currency { get; init; }

    public Money(decimal amount, Currency currency)
    {
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    public static Money Zero(Currency currency) => new(0m, currency);

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot add amounts in different currencies: {left.Currency} and {right.Currency}");
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot subtract amounts in different currencies: {left.Currency} and {right.Currency}");
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator -(Money value) =>
        new(-value.Amount, value.Currency);

    public static Money operator *(Money left, decimal multiplier) =>
        new(left.Amount * multiplier, left.Currency);

    public static Money operator *(decimal multiplier, Money right) =>
        new(multiplier * right.Amount, right.Currency);

    public static Money operator /(Money left, decimal divisor)
    {
        if (divisor == 0m)
            throw new DivideByZeroException();
        return new Money(left.Amount / divisor, left.Currency);
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare amounts in different currencies: {left.Currency} and {right.Currency}");
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare amounts in different currencies: {left.Currency} and {right.Currency}");
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right) =>
        left > right || left == right;

    public static bool operator <=(Money left, Money right) =>
        left < right || left == right;

    public override string ToString() =>
        $"{Amount:N2} {Currency}";
}