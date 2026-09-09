using Microsoft.EntityFrameworkCore;
using XFramework.Application.Contracts.Services;

namespace XFramework.EntityFrameworkCore.Repositories;

public class EfCoreRepository<TEntity, TKey>
    : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected XFrameworkDbContext DbContext { get; }

    protected DbSet<TEntity> DbSet =>
        DbContext.Set<TEntity>();

    public EfCoreRepository(
        XFrameworkDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual async Task<TEntity?> GetAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(
            new object?[] { id },
            cancellationToken);
    }

    public virtual IQueryable<TEntity> GetQueryable()
    {
        return DbSet.AsQueryable();
    }

    public virtual Task<int> CountAsync(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        return query.CountAsync(cancellationToken);
    }

    public virtual Task<List<TEntity>> ToListAsync(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        return query.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(
            entity,
            cancellationToken);
    }

    public virtual Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);

        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);

        return Task.CompletedTask;
    }
}