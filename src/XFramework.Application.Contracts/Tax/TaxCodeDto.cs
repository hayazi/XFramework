using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Tax;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Tax;

public class TaxCodeDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaxType TaxType { get; set; }
    public TaxCalculationMethod CalculationMethod { get; set; }
    public TaxApplication Application { get; set; }
    public decimal Rate { get; set; }
    public MoneyDto? FixedAmount { get; set; }
    public UnitOfMeasure? PerUnit { get; set; }
    public TaxStatus Status { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsRecoverable { get; set; }
    public Guid? AccountId { get; set; }
    public List<TaxTierDto> Tiers { get; set; } = new();
}

public class TaxTierDto
{
    public decimal ThresholdFrom { get; set; }
    public decimal ThresholdTo { get; set; }
    public decimal Rate { get; set; }
}