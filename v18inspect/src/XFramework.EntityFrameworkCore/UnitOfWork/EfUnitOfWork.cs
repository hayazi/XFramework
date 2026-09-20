using XFramework.Application.Abstractions;
using XFramework.EntityFrameworkCore.Persistence;
namespace XFramework.EntityFrameworkCore.UnitOfWork;
public sealed class EfUnitOfWork(XFrameworkDbContext dbContext):IUnitOfWork
{ public Task<int> SaveChangesAsync(CancellationToken ct=default)=>dbContext.SaveChangesAsync(ct); public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct=default)=>new EfUnitOfWorkTransaction(await dbContext.Database.BeginTransactionAsync(ct)); }
