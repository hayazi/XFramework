using Microsoft.EntityFrameworkCore.Storage;
using XFramework.Application.Abstractions;
namespace XFramework.EntityFrameworkCore.UnitOfWork;
public sealed class EfUnitOfWorkTransaction(IDbContextTransaction transaction):IUnitOfWorkTransaction
{ public Task CommitAsync(CancellationToken ct=default)=>transaction.CommitAsync(ct); public Task RollbackAsync(CancellationToken ct=default)=>transaction.RollbackAsync(ct); public ValueTask DisposeAsync()=>transaction.DisposeAsync(); }
