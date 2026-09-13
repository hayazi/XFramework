namespace XFramework.Application.Abstractions;

public interface IUnitOfWork
{
    Task<IUnitOfWorkTransaction> BeginAsync(
        CancellationToken cancellationToken = default);
}
