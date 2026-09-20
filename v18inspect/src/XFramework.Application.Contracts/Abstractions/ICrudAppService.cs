namespace XFramework.Application.Contracts.Abstractions;

public interface ICrudAppService<TEntityDto, in TKey, in TCreateDto, in TUpdateDto>
    : IReadOnlyAppService<TEntityDto, TKey>
{
    Task<TEntityDto> CreateAsync(TCreateDto input, CancellationToken cancellationToken = default);
    Task<TEntityDto> UpdateAsync(TKey id, TUpdateDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}
