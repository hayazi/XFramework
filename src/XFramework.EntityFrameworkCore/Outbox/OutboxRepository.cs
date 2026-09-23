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

    public async Task<bool> RenewLeaseAsync(
        Guid messageId,
        string lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE OutboxMessages
            SET LockedUntilUtc = @LockedUntilUtc
            WHERE Id = @Id
              AND LockId = @LockId
              AND Status = @Processing
              AND LockedUntilUtc > @NowUtc;
            """;

        var affectedRows = await db.Database.ExecuteSqlRawAsync(
            sql,
            [
                new SqlParameter("@LockedUntilUtc", lockedUntilUtc),
                new SqlParameter("@NowUtc", nowUtc),
                new SqlParameter("@Id", messageId),
                new SqlParameter("@LockId", lockId),
                new SqlParameter("@Processing", (byte)OutboxMessageStatus.Processing)
            ],
            cancellationToken);

        return affectedRows == 1;
    }

    public async Task MarkCompletedAsync(
        Guid messageId,
        string lockId,
        DateTime nowUtc,
        DateTime completedOnUtc,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE OutboxMessages
            SET
                Status = @Completed,
                ProcessedOnUtc = @CompletedOnUtc,
                LockId = NULL,
                LockedUntilUtc = NULL
            WHERE Id = @Id
              AND LockId = @LockId
              AND Status = @Processing
              AND LockedUntilUtc > @NowUtc;
            """;

        await db.Database.ExecuteSqlRawAsync(
            sql,
            [
                new SqlParameter("@Completed", (byte)OutboxMessageStatus.Completed),
                new SqlParameter("@CompletedOnUtc", completedOnUtc),
                new SqlParameter("@Id", messageId),
                new SqlParameter("@LockId", lockId),
                new SqlParameter("@Processing", (byte)OutboxMessageStatus.Processing),
                new SqlParameter("@NowUtc", nowUtc)
            ],
            cancellationToken);
    }

    public async Task MarkRetryAsync(
        Guid messageId,
        string lockId,
        DateTime nowUtc,
        DateTime nextAttemptOnUtc,
        string error,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE OutboxMessages
            SET
                Status = @Pending,
                RetryCount = RetryCount + 1,
                NextAttemptOnUtc = @NextAttemptOnUtc,
                LastError = @LastError,
                LockId = NULL,
                LockedUntilUtc = NULL
            WHERE Id = @Id
              AND LockId = @LockId
              AND Status = @Processing
              AND LockedUntilUtc > @NowUtc;
            """;

        await db.Database.ExecuteSqlRawAsync(
            sql,
            [
                new SqlParameter("@Pending", (byte)OutboxMessageStatus.Pending),
                new SqlParameter("@NextAttemptOnUtc", nextAttemptOnUtc),
                new SqlParameter("@LastError", TruncateError(error)),
                new SqlParameter("@Id", messageId),
                new SqlParameter("@LockId", lockId),
                new SqlParameter("@Processing", (byte)OutboxMessageStatus.Processing),
                new SqlParameter("@NowUtc", nowUtc)
            ],
            cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid messageId,
        string lockId,
        DateTime nowUtc,
        string error,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE OutboxMessages
            SET
                Status = @Failed,
                RetryCount = RetryCount + 1,
                LastError = @LastError,
                LockId = NULL,
                LockedUntilUtc = NULL
            WHERE Id = @Id
              AND LockId = @LockId
              AND Status = @Processing
              AND LockedUntilUtc > @NowUtc;
            """;

        await db.Database.ExecuteSqlRawAsync(
            sql,
            [
                new SqlParameter("@Failed", (byte)OutboxMessageStatus.Failed),
                new SqlParameter("@LastError", TruncateError(error)),
                new SqlParameter("@Id", messageId),
                new SqlParameter("@LockId", lockId),
                new SqlParameter("@Processing", (byte)OutboxMessageStatus.Processing),
                new SqlParameter("@NowUtc", nowUtc)
            ],
            cancellationToken);
    }

    public async Task ReleaseExpiredLeasesAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE OutboxMessages
            SET
                Status = @Pending,
                LockId = NULL,
                LockedUntilUtc = NULL
            WHERE Status = @Processing
              AND LockedUntilUtc <= @NowUtc;
            """;

        await db.Database.ExecuteSqlRawAsync(
            sql,
            [
                new SqlParameter("@Pending", (byte)OutboxMessageStatus.Pending),
                new SqlParameter("@NowUtc", nowUtc)
            ],
            cancellationToken);
    }

    private static string TruncateError(string error) =>
        error.Length <= 4000 ? error : error[..4000];
}
