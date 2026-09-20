namespace XFramework.Application.Outbox;
public interface IOutboxProcessor
{
    Task ProcessBatchAsync(
        int batchSize,
        CancellationToken cancellationToken = default);
}