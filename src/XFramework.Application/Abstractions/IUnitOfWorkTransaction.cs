namespace XFramework.Application.Abstractions;

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task CommitAsync(
        CancellationToken cancellationToken = default);

    Task RollbackAsync(
        CancellationToken cancellationToken = default);
}