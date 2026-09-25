using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Inventory;

public interface IKardexAppService : IReadOnlyAppService<KardexEntryDto, Guid>
{
    Task<PagedResult<KardexEntryDto>> GetByItemAsync(Guid itemId, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default);
    Task<PagedResult<KardexEntryDto>> GetByWarehouseAsync(Guid warehouseId, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default);
    Task<PagedResult<KardexEntryDto>> GetByDateRangeAsync(DateTime from, DateTime to, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default);
    Task<KardexEntryDto> CreateReceiptAsync(KardexEntryCreateDto input, CancellationToken cancellationToken = default);
    Task<KardexEntryDto> CreateIssueAsync(KardexEntryCreateDto input, CancellationToken cancellationToken = default);
    Task<KardexEntryDto> CreateAdjustmentAsync(KardexEntryCreateDto input, CancellationToken cancellationToken = default);
    Task<QuantityDto> GetCurrentStockAsync(Guid itemId, Guid warehouseId, CancellationToken cancellationToken = default);
}