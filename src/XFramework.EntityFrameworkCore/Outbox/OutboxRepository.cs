using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using XFramework.EntityFrameworkCore.Persistence;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxRepository(XFrameworkDbContext db) : IOutboxRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
        int batchSize,
        string lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
            return Array.Empty<OutboxMessage>();

        ArgumentException.ThrowIfNullOrWhiteSpace(lockId);

        const string sql = """
            ;WITH cte AS
            (
                SELECT TOP (@BatchSize) *
                FROM OutboxMessages WITH
                (
                    UPDLOCK,
                    READPAST,
                    ROWLOCK,
                    READCOMMITTEDLOCK
                )
                WHERE
                    (
                        Status = @Pending
                        OR
                        (
                            Status = @Processing
                            AND LockedUntilUtc <= @NowUtc
                        )
                    )
                    AND
                    (
                        NextAttemptOnUtc IS NULL
                        OR NextAttemptOnUtc <= @NowUtc
                    )
                ORDER BY CreatedOnUtc, Id
            )
            UPDATE cte
            SET
                Status = @Processing,
                LockId = @LockId,
                LockedUntilUtc = @LockedUntilUtc
            OUTPUT INSERTED.*;
            """;

        var parameters = new[]
        {
            new SqlParameter("@BatchSize", batchSize),
            new SqlParameter("@Pending", (byte)OutboxMessageStatus.Pending),
            new SqlParameter("@Processing", (byte)OutboxMessageStatus.Processing),
            new SqlParameter("@NowUtc", nowUtc),
            new SqlParameter("@LockId", lockId),
            new SqlParameter("@LockedUntilUtc", lockedUntilUtc)
        };

        return await db.OutboxMessages
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task MarkCompletedAsync(
        Guid id,
        string lockId,
        DateTime completed,
        CancellationToken cancellationToken = default)
    {
        var message = await db.OutboxMessages
            .SingleOrDefaultAsync(x => x.Id == id && x.LockId == lockId, cancellationToken);

        if (message is null)
            return;

        message.Status = OutboxMessageStatus.Completed;
        message.ProcessedOnUtc = completed;
        message.LockId = null;
        message.LockedUntilUtc = null;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid id,
        string lockId,
        DateTime next,
        string error,
        CancellationToken cancellationToken = default)
    {
        var message = await db.OutboxMessages
            .SingleOrDefaultAsync(x => x.Id == id && x.LockId == lockId, cancellationToken);

        if (message is null)
            return;

        message.Status = OutboxMessageStatus.Pending;
        message.RetryCount++;
        message.NextAttemptOnUtc = next;
        message.LastError = error;
        message.LockId = null;
        message.LockedUntilUtc = null;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseExpiredLeasesAsync(
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var messages = await db.OutboxMessages
            .Where(x =>
                x.Status == OutboxMessageStatus.Processing &&
                x.LockedUntilUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.Status = OutboxMessageStatus.Pending;
            message.LockId = null;
            message.LockedUntilUtc = null;
        }

        if (messages.Count > 0)
            await db.SaveChangesAsync(cancellationToken);
    }
}
