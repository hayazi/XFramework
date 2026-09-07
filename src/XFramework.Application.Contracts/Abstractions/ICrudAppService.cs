using XFramework.Application.Contracts.Dtos;

namespace XFramework.Application.Contracts.Abstractions;

public interface ICrudAppService<
    TEntityDto,
    in TKey,
    in TCreateDto,
    in TUpdateDto>
    : IReadOnlyAppService<TEntityDto, TKey>, IApplicationService
{
    Task<TEntityDto> CreateAsync(
        TCreateDto input);

    Task<TEntityDto> UpdateAsync(
        TKey id,
        TUpdateDto input);

    Task DeleteAsync(
        TKey id);
}