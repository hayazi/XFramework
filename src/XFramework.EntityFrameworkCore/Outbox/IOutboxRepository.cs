namespace XFramework.EntityFrameworkCore.Outbox;

public interface IOutboxRepository
{
    Task<IReadOnlyList<OutboxMessage>> ClaimPendingMessagesAsync(
            int batchSize,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default);

    Task MarkAsProcessingAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default);

    Task MarkAsCompletedAsync(
        OutboxMessage message,
        string lockId,
        CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(
        OutboxMessage message,
        string lockId,
        string error,
        DateTime nextAttemptOnUtc,
        CancellationToken cancellationToken = default);
}