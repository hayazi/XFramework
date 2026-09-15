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
    public async Task<IReadOnlyList<OutboxMessage>>        ClaimBatchAsync(
            int batchSize,
            string lockId,
            DateTime nowUtc,
            DateTime lockedUntilUtc)
    {
        var sql = $@"
        ;WITH cte AS
        (
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
                    OR
                    (
                        Status = {(int)OutboxMessageStatus.Processing}
                        AND LockedUntilUtc < @Now
                    )
                )
                AND
                (
                    NextAttemptOnUtc IS NULL
                    OR NextAttemptOnUtc <= {now}
                )
            ORDER BY CreatedOnUtc
        )

        UPDATE cte
        SET
            Status = {(int)OutboxMessageStatus.Processing},
            LockId = {lockId},
            LockedUntilUtc = {LockedUntilUtc}

        OUTPUT INSERTED.*;
        ";

        return await _dbContext.OutboxMessages
            .FromSqlInterpolated(
                sql
                // ,
                // parameters
                )
            .ToListAsync(cancellationToken);
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
        /************
        UPDATE OutboxMessages
        SET
            Status = @Completed,

            ProcessedOnUtc = @Now,

            LockId = NULL,

            LockedUntilUtc = NULL
        WHERE
            Id = @Id
            AND LockId = @LockId
        ***********/
        // message.Status = OutboxMessageStatus.Completed;
        // message.ProcessedOnUtc = DateTime.UtcNow;
        // message.LastError = null;

        // await _dbContext.SaveChangesAsync(
        //     cancellationToken);
    }

    public async Task MarkAsFailedAsync(
        OutboxMessage message,
        string error,
        DateTime nextAttemptOnUtc,
        CancellationToken cancellationToken = default)
    {
        /***************
        UPDATE OutboxMessages
        SET
            Status = @Pending,

            RetryCount = RetryCount + 1,

            NextAttemptOnUtc = @NextAttempt,

            Error = @Error,

            LockId = NULL,

            LockedUntilUtc = NULL
        WHERE
            Id = @Id
            AND LockId = @LockId
        ***************/
        // message.Status = OutboxMessageStatus.Pending;
        // message.RetryCount++;
        // message.LastError = error;
        // message.NextAttemptOnUtc = nextAttemptOnUtc;

        // await _dbContext.SaveChangesAsync(
        //     cancellationToken);
    }

    public async Task MarkAsReleaseExpiredLeasesAsync(
        OutboxMessage message,
        string error,
        DateTime nextAttemptOnUtc,
        CancellationToken cancellationToken = default)
    {
/**************
UPDATE OutboxMessages
SET
    Status = @Pending,

    LockId = NULL,

    LockedUntilUtc = NULL
WHERE
    Status = @Processing
    AND LockedUntilUtc < @Now
***************/
    }
}