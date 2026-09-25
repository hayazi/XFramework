using Microsoft.EntityFrameworkCore;
using XFramework.Application.Abstractions;
using XFramework.Domain.Entities;
using XFramework.EntityFrameworkCore.Persistence;
namespace XFramework.EntityFrameworkCore.Repositories;
public class EfCoreRepository<TEntity,TKey>(XFrameworkDbContext dbContext):IRepository<TEntity,TKey> where TEntity:Entity<TKey>
{ 
    protected XFrameworkDbContext DbContext{get;}=dbContext; 
    protected DbSet<TEntity> DbSet=>DbContext.Set<TEntity>();
    public Task<TEntity?> GetAsync(TKey id,CancellationToken ct=default)=>DbSet.FindAsync([id],ct).AsTask();
    public Task<TEntity?> FirstOrDefaultAsync(IQueryable<TEntity> q,CancellationToken ct=default)=>q.FirstOrDefaultAsync(ct);
    public Task<TEntity?> SingleOrDefaultAsync(IQueryable<TEntity> q,CancellationToken ct=default)=>q.SingleOrDefaultAsync(ct);
    public IQueryable<TEntity> GetQueryable()=>DbSet.AsQueryable();
    public Task<int> CountAsync(IQueryable<TEntity> q,CancellationToken ct=default)=>q.CountAsync(ct);
    public Task<List<TEntity>> ToListAsync(IQueryable<TEntity> q,CancellationToken ct=default)=>q.ToListAsync(ct);
    public async Task AddAsync(TEntity e,CancellationToken ct=default)=>await DbSet.AddAsync(e,ct);
    public Task UpdateAsync(TEntity e,CancellationToken ct=default){DbSet.Update(e);return Task.CompletedTask;}
    public Task DeleteAsync(TEntity e,CancellationToken ct=default){DbSet.Remove(e);return Task.CompletedTask;}
}
