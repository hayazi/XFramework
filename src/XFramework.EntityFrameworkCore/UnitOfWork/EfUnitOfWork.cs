using XFramework.Application.Abstractions;

namespace XFramework.EntityFrameworkCore.UnitOfWork;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly XFrameworkDbContext _dbContext;

    public EfUnitOfWork(
        XFrameworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IUnitOfWorkTransaction>
        BeginTransactionAsync(
            CancellationToken cancellationToken = default)
    {
        var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        return new EfUnitOfWorkTransaction(transaction);
    }
}