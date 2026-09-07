using Microsoft.EntityFrameworkCore;
using XFramework.Application.Abstractions;
using XFramework.Core.Domain;

namespace XFramework.EntityFrameworkCore.Repositories;

public class EfRepository<TEntity, TKey>
    : IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    protected XFrameworkDbContext DbContext { get; }

    protected DbSet<TEntity> DbSet =>
        DbContext.Set<TEntity>();

    public EfRepository(
        XFrameworkDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual async Task<TEntity?> GetAsync(
        TKey id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<List<TEntity>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string? sorting)
    {
        var query = DbSet.AsQueryable();

        query = ApplySorting(query, sorting);

        return await query
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();
    }

    public virtual async Task<int> CountAsync()
    {
        return await DbSet.CountAsync();
    }

    public virtual async Task InsertAsync(
        TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    public virtual Task UpdateAsync(
        TEntity entity)
    {
        DbSet.Update(entity);

        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(
        TEntity entity)
    {
        DbSet.Remove(entity);

        return Task.CompletedTask;
    }

    protected virtual IQueryable<TEntity> ApplySorting(
        IQueryable<TEntity> query,
        string? sorting)
    {
        return query;
    }
}