using XFramework.Application.Contracts.Abstractions;
using XFramework.Domain.Dimensions;

namespace XFramework.Application.Contracts.Dimensions;

public interface ICostCenterAppService : ICrudAppService<CostCenterDto, Guid, CostCenterCreateDto, CostCenterUpdateDto>
{
    Task<CostCenterDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<CostCenterDto>> GetHierarchyAsync(CancellationToken cancellationToken = default);
    Task<List<CostCenterDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<CostCenterDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CostCenterDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CostCenterDto> CloseAsync(Guid id, CancellationToken cancellationToken = default);
}