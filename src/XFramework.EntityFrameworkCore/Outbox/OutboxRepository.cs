using Microsoft.EntityFrameworkCore;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly XFrameworkDbContext _dbContext;

    public OutboxRepository(
        XFrameworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OutboxMessage>> ClaimPendingMessagesAsync(
            int batchSize,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
    {
        var lockId = Guid.NewGuid().ToString("N");

        var now = DateTime.UtcNow;

        var lockedUntil = now.Add(leaseDuration);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var messages = await _dbContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT TOP ({batchSize}) *
                FROM OutboxMessages WITH
                (
                    UPDLOCK,
                    READPAST,
                    ROWLOCK
                )
                WHERE
                    (
                        Status = {(int)OutboxMessageStatus.Pending}
                        AND
                        (
                            NextAttemptOnUtc IS NULL
                            OR NextAttemptOnUtc <= {now}
                        )
                    )
                    OR
                    (
                        Status = {(int)OutboxMessageStatus.Processing}
                        AND
                        LockedUntilUtc < {now}
                    )
                ORDER BY CreatedOnUtc
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.Status =
                OutboxMessageStatus.Processing;

            message.LockId = lockId;

            message.LockedUntilUtc =
                lockedUntil;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return messages;
    }
    // public async Task<IReadOnlyList<OutboxMessage>>
    //     GetPendingMessagesAsync(
    //         int batchSize,
    //         CancellationToken cancellationToken = default)
    // {
    //     var now = DateTime.UtcNow;

    //     return await _dbContext.OutboxMessages
    //         .Where(x =>
    //             x.Status == OutboxMessageStatus.Pending &&
    //             (x.NextAttemptOnUtc == null ||
    //              x.NextAttemptOnUtc <= now))
    //         .OrderBy(x => x.CreatedOnUtc)
    //         .Take(batchSize)
    //         .ToListAsync(cancellationToken);
    // }

    public async Task MarkAsProcessingAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        message.Status = OutboxMessageStatus.Processing;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task MarkAsCompletedAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        message.Status = OutboxMessageStatus.Completed;
        message.ProcessedOnUtc = DateTime.UtcNow;
        message.LastError = null;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task MarkAsFailedAsync(
        OutboxMessage message,
        string error,
        DateTime nextAttemptOnUtc,
        CancellationToken cancellationToken = default)
    {
        message.Status = OutboxMessageStatus.Pending;
        message.RetryCount++;
        message.LastError = error;
        message.NextAttemptOnUtc = nextAttemptOnUtc;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}