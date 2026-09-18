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
