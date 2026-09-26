using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Inventory;

namespace XFramework.Application.Contracts.Inventory;

public interface ICardexAppService : IReadOnlyAppService<CardexEntryDto, Guid>
{
    Task<PagedResult<CardexEntryDto>> GetByItemAsync(Guid itemId, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default);
    Task<PagedResult<CardexEntryDto>> GetByWarehouseAsync(Guid warehouseId, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default);
    Task<PagedResult<CardexEntryDto>> GetByDateRangeAsync(DateTime from, DateTime to, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default);
    Task<CardexEntryDto> CreateReceiptAsync(CardexEntryCreateDto input, CancellationToken cancellationToken = default);
    Task<CardexEntryDto> CreateIssueAsync(CardexEntryCreateDto input, CancellationToken cancellationToken = default);
    Task<CardexEntryDto> CreateAdjustmentAsync(CardexEntryCreateDto input, CancellationToken cancellationToken = default);
    Task<MoneyDto> GetCurrentStockAsync(Guid itemId, Guid warehouseId, CancellationToken cancellationToken = default);
}