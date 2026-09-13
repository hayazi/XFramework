using Microsoft.EntityFrameworkCore;
using XFramework.Application.Abstractions;

namespace XFramework.Infrastructure.Persistence;

public sealed class EfUnitOfWork(
    XFrameworkDbContext db) : IUnitOfWork
{
    public async Task<IUnitOfWorkTransaction> BeginAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction = await db.Database.BeginTransactionAsync(
            cancellationToken);

        return new EfUnitOfWorkTransaction(db, transaction);
    }

    private sealed class EfUnitOfWorkTransaction(
        XFrameworkDbContext db,
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
        : IUnitOfWorkTransaction
    {
        private bool _completed;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return db.SaveChangesAsync(cancellationToken);
        }

        public async Task CommitAsync(
            CancellationToken cancellationToken = default)
        {
            if (_completed)
                return;

            await transaction.CommitAsync(cancellationToken);

            _completed = true;
        }

        public async Task RollbackAsync(
            CancellationToken cancellationToken = default)
        {
            if (_completed)
                return;

            await transaction.RollbackAsync(cancellationToken);

            _completed = true;
        }

        public async ValueTask DisposeAsync()
        {
            await transaction.DisposeAsync();
        }
    }
}