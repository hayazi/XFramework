namespace XFramework.EntityFrameworkCore.Outbox;

public interface IOutboxRepository
{
    Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
        int batchSize,
        string lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        CancellationToken cancellationToken = default);

    Task MarkCompletedAsync(
        Guid messageId,
        string lockId,
        DateTime completedOnUtc,
        CancellationToken cancellationToken = default);

    Task MarkRetryAsync(
        Guid messageId,
        string lockId,
        DateTime nextAttemptOnUtc,
        string error,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        Guid messageId,
        string lockId,
        string error,
        CancellationToken cancellationToken = default);

    Task ReleaseExpiredLeasesAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
