using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Contracts.Tax;
using XFramework.Application.Services;
using XFramework.Domain.Tax;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Tax;

[Validate]
public sealed class TaxCodeAppService(IRepository<TaxCode, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<TaxCode, TaxCodeDto, Guid, TaxCodeCreateDto, TaxCodeUpdateDto>(repository, unitOfWork), ITaxCodeAppService
{
    protected override TaxCodeDto MapToDto(TaxCode e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        TaxType = e.TaxType,
        CalculationMethod = e.CalculationMethod,
        Application = e.Application,
        Rate = e.Rate.Value,
        FixedAmount = e.FixedAmount != null ? new MoneyDto { Amount = e.FixedAmount.Value.Amount, Currency = e.FixedAmount.Value.Currency } : null,
        PerUnit = e.PerUnit,
        Status = e.Status,
        EffectiveFrom = e.EffectiveFrom,
        EffectiveTo = e.EffectiveTo,
        IsDefault = e.IsDefault,
        IsRecoverable = e.IsRecoverable,
        AccountId = e.AccountId,
        Tiers = e.Tiers.Select(t => new TaxTierDto { ThresholdFrom = t.ThresholdFrom, ThresholdTo = t.ThresholdTo, Rate = t.Rate.Value }).ToList()
    };

    protected override Task<TaxCode> MapToEntityAsync(TaxCodeCreateDto i, CancellationToken ct)
    {
        var tiers = i.Tiers?.Select(t => new TaxTier(t.ThresholdFrom, t.ThresholdTo, new Percentage(t.Rate))).ToList() ?? new List<TaxTier>();
        
        var taxCode = TaxCode.Create(
            i.Code, i.Name, i.TaxType, i.CalculationMethod, i.Application, 
            new Percentage(i.Rate), i.EffectiveFrom, i.Description, i.EffectiveTo,
            i.FixedAmount != null ? new Money(i.FixedAmount.Amount, i.FixedAmount.Currency) : null,
            i.PerUnit, i.IsDefault, i.IsRecoverable, i.AccountId, tiers);
        
        return Task.FromResult(taxCode);
    }

    protected override Task MapToEntityAsync(TaxCodeUpdateDto i, TaxCode e, CancellationToken ct)
    {
        var tiers = i.Tiers?.Select(t => new TaxTier(t.ThresholdFrom, t.ThresholdTo, new Percentage(t.Rate))).ToList() ?? new List<TaxTier>();
        
        e.UpdateDetails(
            i.Name, i.Description, i.TaxType, i.CalculationMethod, i.Application,
            i.Rate.HasValue ? new Percentage(i.Rate.Value) : null,
            i.FixedAmount != null ? new Money(i.FixedAmount.Amount, i.FixedAmount.Currency) : null,
            i.PerUnit, i.Status, i.EffectiveFrom, i.EffectiveTo,
            i.IsDefault, i.IsRecoverable, i.AccountId, tiers);
        return Task.CompletedTask;
    }

    public async Task<TaxCodeDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<TaxCodeDto?> GetDefaultAsync(TaxType taxType, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.TaxType == taxType && x.IsDefault && x.Status == TaxStatus.Active);
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<TaxCodeDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Status == TaxStatus.Active);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<TaxCodeDto>> GetByTypeAsync(TaxType taxType, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.TaxType == taxType);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<MoneyDto> CalculateTaxAsync(Guid taxCodeId, MoneyDto baseAmount, decimal? quantity = null, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(taxCodeId, cancellationToken) ?? throw new KeyNotFoundException($"TaxCode with id '{taxCodeId}' was not found.");
        var tax = entity.CalculateTax(new Money(baseAmount.Amount, baseAmount.Currency), quantity.HasValue ? new Quantity(quantity.Value, UnitOfMeasure.Piece) : null);
        return new MoneyDto { Amount = tax.Amount, Currency = tax.Currency };
    }

    public async Task<TaxCodeDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"TaxCode with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<TaxCodeDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"TaxCode with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}