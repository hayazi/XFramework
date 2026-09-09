using System.Linq.Expressions;

namespace XFramework.Application.Contracts.Services;

public interface IRepository<TEntity, in TKey>
    where TEntity : class
{
    Task<TEntity?> GetAsync(
        TKey id,
        CancellationToken cancellationToken = default);

    IQueryable<TEntity> GetQueryable();

    Task<int> CountAsync(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default);

    Task<List<TEntity>> ToListAsync(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);
}