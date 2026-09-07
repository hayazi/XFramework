using XFramework.Core.Results;
using XFramework.Application.Contracts.Dtos;
namespace XFramework.Application.Contracts.Abstractions;

public interface IReadOnlyAppService<TEntityDto, in TKey>
    : IApplicationService
{
    Task<TEntityDto?> GetAsync(TKey id);

    Task<PagedResult<TEntityDto>> GetListAsync(
        PagedAndSortedRequestDto input);
}