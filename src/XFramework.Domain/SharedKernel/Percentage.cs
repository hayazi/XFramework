namespace XFramework.Domain.SharedKernel;

public readonly record struct Percentage : IEquatable<Percentage>
{
    public decimal Value { get; init; }

    public Percentage(decimal value)
    {
        if (value < 0m || value > 100m)
            throw new ArgumentOutOfRangeException(nameof(value), "Percentage must be between 0 and 100.");
        Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static Percentage Zero => new(0m);
    public static Percentage Hundred => new(100m);

    public static Percentage operator +(Percentage left, Percentage right) =>
        new(left.Value + right.Value);

    public static Percentage operator -(Percentage left, Percentage right) =>
        new(left.Value - right.Value);

    public static Percentage operator *(Percentage left, decimal multiplier) =>
        new(left.Value * multiplier);

    public static Percentage operator *(decimal multiplier, Percentage right) =>
        new(multiplier * right.Value);

    public static Money operator *(Percentage left, Money right) =>
        new(right.Amount * left.Value / 100m, right.Currency);

    public static Money operator *(Money left, Percentage right) =>
        new(left.Amount * right.Value / 100m, left.Currency);

    public static Quantity operator *(Percentage left, Quantity right) =>
        new(right.Value * left.Value / 100m, right.Unit);

    public override string ToString() =>
        $"{Value:N2}%";
}