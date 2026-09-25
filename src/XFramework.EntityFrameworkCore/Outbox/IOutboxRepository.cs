namespace XFramework.EntityFrameworkCore.Outbox;

public interface IOutboxRepository
{
    Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
        int batchSize,
        string lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        CancellationToken cancellationToken = default);

    Task<bool> RenewLeaseAsync(
        Guid messageId,
        string lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        CancellationToken cancellationToken = default);

    Task<bool> MarkCompletedAsync(
        Guid messageId,
        string lockId,
        DateTime nowUtc,
        DateTime completedOnUtc,
        CancellationToken cancellationToken = default);

    Task<bool> MarkRetryAsync(
        Guid messageId,
        string lockId,
        DateTime nowUtc,
        DateTime nextAttemptOnUtc,
        string error,
        CancellationToken cancellationToken = default);

    Task<bool> MarkFailedAsync(
        Guid messageId,
        string lockId,
        DateTime nowUtc,
        string error,
        CancellationToken cancellationToken = default);

    Task<int> ReleaseExpiredLeasesAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessage>> GetFailedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessage>> GetProcessingAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<OutboxMessage?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<int> GetCountByStatusAsync(
        OutboxMessageStatus status,
        CancellationToken cancellationToken = default);
}
