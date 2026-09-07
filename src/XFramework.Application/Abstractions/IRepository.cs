using XFramework.Core.Domain;

namespace XFramework.Application.Abstractions;

public interface IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    Task<TEntity?> GetAsync(TKey id);

    Task<List<TEntity>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string? sorting);

    Task<int> CountAsync();

    Task InsertAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);
}