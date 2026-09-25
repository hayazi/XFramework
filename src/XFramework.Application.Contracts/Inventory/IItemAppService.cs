using XFramework.Application.Contracts.Abstractions;
using XFramework.Domain.Inventory;

namespace XFramework.Application.Contracts.Inventory;

public interface IItemAppService : ICrudAppService<ItemDto, Guid, ItemCreateDto, ItemUpdateDto>
{
    Task<ItemDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<ItemDto>> GetByTypeAsync(ItemType type, CancellationToken cancellationToken = default);
    Task<List<ItemDto>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<ItemDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ItemDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ItemDto> DiscontinueAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ItemDto> BlockAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ItemDto> UnblockAsync(Guid id, CancellationToken cancellationToken = default);
}