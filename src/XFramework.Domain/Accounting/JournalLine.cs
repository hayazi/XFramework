using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Accounting;

public readonly record struct JournalLine : IEquatable<JournalLine>
{
    public Guid AccountId { get; init; }
    public AccountNature Side { get; init; }
    public Money Amount { get; init; }
    public string Description { get; init; }
    public Guid? DimensionValueId { get; init; }

    public JournalLine(
        Guid accountId,
        AccountNature side,
        Money amount,
        string description = "",
        Guid? dimensionValueId = null)
    {
        AccountId = accountId;
        Side = side;
        Amount = amount;
        Description = description ?? string.Empty;
        DimensionValueId = dimensionValueId;
    }

    public override string ToString() =>
        $"{Side} {Amount} - {Description}";
}