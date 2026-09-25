namespace XFramework.Domain.SharedKernel;

public readonly record struct Quantity : IEquatable<Quantity>
{
    public decimal Value { get; init; }
    public UnitOfMeasure Unit { get; init; }

    public Quantity(decimal value, UnitOfMeasure unit)
    {
        Value = decimal.Round(value, 4, MidpointRounding.AwayFromZero);
        Unit = unit;
    }

    public static Quantity Zero(UnitOfMeasure unit) => new(0m, unit);

    public static Quantity operator +(Quantity left, Quantity right)
    {
        if (left.Unit != right.Unit)
            throw new InvalidOperationException($"Cannot add quantities with different units: {left.Unit} and {right.Unit}");
        return new Quantity(left.Value + right.Value, left.Unit);
    }

    public static Quantity operator -(Quantity left, Quantity right)
    {
        if (left.Unit != right.Unit)
            throw new InvalidOperationException($"Cannot subtract quantities with different units: {left.Unit} and {right.Unit}");
        return new Quantity(left.Value - right.Value, left.Unit);
    }

    public static Quantity operator *(Quantity left, decimal multiplier) =>
        new(left.Value * multiplier, left.Unit);

    public static Quantity operator *(decimal multiplier, Quantity right) =>
        new(multiplier * right.Value, right.Unit);

    public static Quantity operator /(Quantity left, decimal divisor)
    {
        if (divisor == 0m)
            throw new DivideByZeroException();
        return new Quantity(left.Value / divisor, left.Unit);
    }

    public override string ToString() =>
        $"{Value:N4} {Unit}";
}