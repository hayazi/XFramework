using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Tax;

namespace XFramework.Application.Contracts.Tax;

public interface ITaxCodeAppService : ICrudAppService<TaxCodeDto, Guid, TaxCodeCreateDto, TaxCodeUpdateDto>
{
    Task<TaxCodeDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<TaxCodeDto?> GetDefaultAsync(TaxType taxType, CancellationToken cancellationToken = default);
    Task<List<TaxCodeDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<TaxCodeDto>> GetByTypeAsync(TaxType taxType, CancellationToken cancellationToken = default);
    Task<MoneyDto> CalculateTaxAsync(Guid taxCodeId, MoneyDto baseAmount, decimal? quantity = null, CancellationToken cancellationToken = default);
    Task<TaxCodeDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaxCodeDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}