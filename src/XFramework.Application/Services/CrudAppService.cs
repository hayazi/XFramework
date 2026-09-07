using XFramework.Application.Abstractions;
using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Core.Domain;
using XFramework.Core.Exceptions;

namespace XFramework.Application.Services;

public abstract class CrudAppService<
    TEntity,
    TEntityDto,
    TKey,
    TCreateDto,
    TUpdateDto>
    : ICrudAppService<
        TEntityDto,
        TKey,
        TCreateDto,
        TUpdateDto>
    where TEntity : Entity<TKey>
{
    protected IRepository<TEntity, TKey> Repository { get; }
    protected IUnitOfWork UnitOfWork { get; }
    protected CrudAppService(
        IRepository<TEntity, TKey> repository,
        IUnitOfWork unitOfWork)
    {
        Repository = repository;
        UnitOfWork = unitOfWork;
    }

    public virtual async Task<TEntityDto?> GetAsync(TKey id)
    {
        var entity = await Repository.GetAsync(id);

        if (entity is null)
            return default;

        return MapToDto(entity);
    }

    public virtual async Task<PagedResult<TEntityDto>> GetListAsync(
        PagedAndSortedRequestDto input)
    {
        var totalCount = await Repository.CountAsync();

        var entities = await Repository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting);

        var items = entities
            .Select(MapToDto)
            .ToList();

        return new PagedResult<TEntityDto>(
            totalCount,
            items);
    }

    public virtual async Task<TEntityDto> CreateAsync(
        TCreateDto input)
    {
        var entity = await MapToEntityAsync(input);

        await Repository.InsertAsync(entity);

        await UnitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    public virtual async Task<TEntityDto> UpdateAsync(
        TKey id,
        TUpdateDto input)
    {
        var entity = await Repository.GetAsync(id);

        if (entity is null)
        {
           throw new BusinessException(
            "Entity.NotFound",
            $"Entity with id '{id}' was not found.");
        }

        await MapToEntityAsync(input, entity);

        await Repository.UpdateAsync(entity);

        await UnitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    public virtual async Task DeleteAsync(TKey id)
    {
        var entity = await Repository.GetAsync(id);

        if (entity is null)
        {
            throw new BusinessException(
                "Entity.NotFound",
                $"Entity with id '{id}' was not found.");
        }

        await Repository.DeleteAsync(entity);

        await UnitOfWork.SaveChangesAsync();
    }

    protected abstract TEntityDto MapToDto(
        TEntity entity);

    protected abstract Task<TEntity> MapToEntityAsync(
        TCreateDto input);

    protected abstract Task MapToEntityAsync(
        TUpdateDto input,
        TEntity entity);
}