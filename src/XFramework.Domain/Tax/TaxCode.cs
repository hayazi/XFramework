using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Tax;

public sealed class TaxCode : AggregateRoot<Guid>
{
    private TaxCode() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaxType TaxType { get; private set; }
    public TaxCalculationMethod CalculationMethod { get; private set; }
    public TaxApplication Application { get; private set; }
    public Percentage Rate { get; private set; }
    public Money? FixedAmount { get; private set; }
    public UnitOfMeasure? PerUnit { get; private set; }
    public TaxStatus Status { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsRecoverable { get; private set; }
    public Guid? AccountId { get; private set; }
    public List<TaxTier> Tiers { get; private set; } = new();

    public static TaxCode Create(
        string code,
        string name,
        TaxType taxType,
        TaxCalculationMethod calculationMethod,
        TaxApplication application,
        Percentage rate,
        DateTime effectiveFrom,
        string? description = null,
        DateTime? effectiveTo = null,
        Money? fixedAmount = null,
        UnitOfMeasure? perUnit = null,
        bool isDefault = false,
        bool isRecoverable = false,
        Guid? accountId = null,
        List<TaxTier>? tiers = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Tax code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tax name is required.", nameof(name));
        if (calculationMethod == TaxCalculationMethod.FixedAmount && !fixedAmount.HasValue)
            throw new ArgumentException("Fixed amount is required for fixed amount calculation method.", nameof(fixedAmount));
        if (calculationMethod == TaxCalculationMethod.Tiered && (tiers == null || tiers.Count == 0))
            throw new ArgumentException("At least one tier is required for tiered calculation method.", nameof(tiers));

        var taxCode = new TaxCode
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            TaxType = taxType,
            CalculationMethod = calculationMethod,
            Application = application,
            Rate = rate,
            FixedAmount = fixedAmount,
            PerUnit = perUnit,
            Status = TaxStatus.Active,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsDefault = isDefault,
            IsRecoverable = isRecoverable,
            AccountId = accountId,
            Tiers = tiers ?? new List<TaxTier>()
        };

        taxCode.AddDomainEvent(new TaxCodeCreated(taxCode.Id, taxCode.Code, taxCode.Name, taxCode.TaxType));
        return taxCode;
    }

    public Money CalculateTax(Money baseAmount, Quantity? quantity = null)
    {
        if (Status != TaxStatus.Active)
            throw new InvalidOperationException($"Tax code {Code} is not active.");
        
        if (DateTime.UtcNow < EffectiveFrom || (EffectiveTo.HasValue && DateTime.UtcNow > EffectiveTo.Value))
            throw new InvalidOperationException($"Tax code {Code} is not effective for current date.");

        return CalculationMethod switch
        {
            TaxCalculationMethod.Percentage => baseAmount * Rate.Value / 100,
            TaxCalculationMethod.FixedAmount => FixedAmount!.Value * (quantity?.Value ?? 1),
            TaxCalculationMethod.Tiered => CalculateTieredTax(baseAmount),
            _ => throw new InvalidOperationException($"Unsupported calculation method: {CalculationMethod}")
        };
    }

    private Money CalculateTieredTax(Money baseAmount)
    {
        var tax = Money.Zero(baseAmount.Currency);
        var remainingAmount = baseAmount.Amount;

        foreach (var tier in Tiers.OrderBy(t => t.ThresholdFrom))
        {
            if (remainingAmount <= 0) break;
            
            var tierAmount = Math.Min(remainingAmount, tier.ThresholdTo - tier.ThresholdFrom);
            if (tierAmount <= 0) continue;

            tax += new Money(tierAmount, baseAmount.Currency) * tier.Rate.Value / 100;
            remainingAmount -= tierAmount;
        }

        return tax;
    }

    public void UpdateDetails(
        string name,
        string? description,
        TaxType? taxType = null,
        TaxCalculationMethod? calculationMethod = null,
        TaxApplication? application = null,
        Percentage? rate = null,
        Money? fixedAmount = null,
        UnitOfMeasure? perUnit = null,
        TaxStatus? status = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveTo = null,
        bool? isDefault = null,
        bool? isRecoverable = null,
        Guid? accountId = null,
        List<TaxTier>? tiers = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        
        if (taxType.HasValue) TaxType = taxType.Value;
        if (calculationMethod.HasValue)
        {
            if (calculationMethod.Value == TaxCalculationMethod.FixedAmount && !fixedAmount.HasValue && !FixedAmount.HasValue)
                throw new ArgumentException("Fixed amount is required for fixed amount calculation method.");
            if (calculationMethod.Value == TaxCalculationMethod.Tiered && (tiers == null || tiers.Count == 0) && Tiers.Count == 0)
                throw new ArgumentException("At least one tier is required for tiered calculation method.");
            CalculationMethod = calculationMethod.Value;
        }
        if (application.HasValue) Application = application.Value;
        if (rate.HasValue) Rate = rate.Value;
        if (fixedAmount.HasValue) FixedAmount = fixedAmount.Value;
        if (perUnit.HasValue) PerUnit = perUnit.Value;
        if (status.HasValue) Status = status.Value;
        if (effectiveFrom.HasValue) EffectiveFrom = effectiveFrom.Value;
        if (effectiveTo.HasValue) EffectiveTo = effectiveTo.Value;
        if (isDefault.HasValue) IsDefault = isDefault.Value;
        if (isRecoverable.HasValue) IsRecoverable = isRecoverable.Value;
        if (accountId.HasValue) AccountId = accountId.Value;
        if (tiers != null) Tiers = tiers;

        AddDomainEvent(new TaxCodeUpdated(Id, Code, Name));
    }

    public void Activate()
    {
        if (Status == TaxStatus.Active) return;
        Status = TaxStatus.Active;
        AddDomainEvent(new TaxCodeActivated(Id));
    }

    public void Deactivate()
    {
        if (Status == TaxStatus.Inactive) return;
        Status = TaxStatus.Inactive;
        AddDomainEvent(new TaxCodeDeactivated(Id));
    }
}