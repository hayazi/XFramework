using XFramework.Application.Contracts.Abstractions;
using XFramework.Domain.Inventory;

namespace XFramework.Application.Contracts.Inventory;

public interface IWarehouseAppService : ICrudAppService<WarehouseDto, Guid, WarehouseCreateDto, WarehouseUpdateDto>
{
    Task<WarehouseDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<WarehouseDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WarehouseDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}