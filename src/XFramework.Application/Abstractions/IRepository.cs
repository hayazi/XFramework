using XFramework.Domain.Entities;

namespace XFramework.Application.Abstractions;

public interface IRepository<TEntity, in TKey> where TEntity : Entity<TKey>
{
    Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default);
    Task<TEntity?> SingleOrDefaultAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default);
    IQueryable<TEntity> GetQueryable();
    Task<int> CountAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default);
    Task<List<TEntity>> ToListAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
