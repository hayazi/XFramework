using XFramework.Application.Contracts.Abstractions;
using XFramework.Domain.Dimensions;

namespace XFramework.Application.Contracts.Dimensions;

public interface ICustomDimensionAppService : ICrudAppService<CustomDimensionDto, Guid, CustomDimensionCreateDto, CustomDimensionUpdateDto>
{
    Task<CustomDimensionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<CustomDimensionDto?> GetByDimensionKeyAsync(string dimensionKey, CancellationToken cancellationToken = default);
    Task<CustomDimensionDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomDimensionDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}