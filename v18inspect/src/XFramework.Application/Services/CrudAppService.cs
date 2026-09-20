using XFramework.Application.Abstractions;
using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Entities;

namespace XFramework.Application.Services;

public abstract class CrudAppService<TEntity,TDto,TKey,TCreateDto,TUpdateDto> : ICrudAppService<TDto,TKey,TCreateDto,TUpdateDto>
    where TEntity : Entity<TKey>
{
    protected IRepository<TEntity,TKey> Repository { get; }
    protected IUnitOfWork UnitOfWork { get; }

    protected CrudAppService(IRepository<TEntity,TKey> repository, IUnitOfWork unitOfWork)
    { Repository=repository; UnitOfWork=unitOfWork; }

    public virtual async Task<TDto?> GetAsync(TKey id, CancellationToken cancellationToken=default)
    {
        var entity=await Repository.GetAsync(id,cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public virtual async Task<PagedResult<TDto>> GetListAsync(PagedAndSortedRequestDto input, CancellationToken cancellationToken=default)
    {
        input ??= new();
        var query=Repository.GetQueryable();
        query=ApplySorting(query,input.Sorting);
        var total=await Repository.CountAsync(query,cancellationToken);
        var entities=await Repository.ToListAsync(query.Skip(Math.Max(0,input.SkipCount)).Take(Math.Max(1,input.MaxResultCount)),cancellationToken);
        return new PagedResult<TDto>(entities.Select(MapToDto).ToList(),total);
    }

    public virtual async Task<TDto> CreateAsync(TCreateDto input, CancellationToken cancellationToken=default)
    {
        await ValidateCreateAsync(input,cancellationToken);
        var entity=await MapToEntityAsync(input,cancellationToken);
        await Repository.AddAsync(entity,cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public virtual async Task<TDto> UpdateAsync(TKey id,TUpdateDto input,CancellationToken cancellationToken=default)
    {
        var entity=await Repository.GetAsync(id,cancellationToken) ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' was not found.");
        await ValidateUpdateAsync(entity,input,cancellationToken);
        await MapToEntityAsync(input,entity,cancellationToken);
        await Repository.UpdateAsync(entity,cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public virtual async Task DeleteAsync(TKey id,CancellationToken cancellationToken=default)
    {
        var entity=await Repository.GetAsync(id,cancellationToken) ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' was not found.");
        await Repository.DeleteAsync(entity,cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query,string? sorting)=>query;
    protected virtual Task ValidateCreateAsync(TCreateDto input,CancellationToken ct)=>Task.CompletedTask;
    protected virtual Task ValidateUpdateAsync(TEntity entity,TUpdateDto input,CancellationToken ct)=>Task.CompletedTask;
    protected abstract TDto MapToDto(TEntity entity);
    protected abstract Task<TEntity> MapToEntityAsync(TCreateDto input,CancellationToken ct);
    protected abstract Task MapToEntityAsync(TUpdateDto input,TEntity entity,CancellationToken ct);
}
