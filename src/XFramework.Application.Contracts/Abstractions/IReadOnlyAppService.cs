using XFramework.Application.Contracts.Dtos;

namespace XFramework.Application.Contracts.Abstractions;

public interface IReadOnlyAppService<TEntityDto, in TKey> : IApplicationService
{
    Task<TEntityDto?> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<PagedResult<TEntityDto>> GetListAsync(PagedAndSortedRequestDto input, CancellationToken cancellationToken = default);
}
