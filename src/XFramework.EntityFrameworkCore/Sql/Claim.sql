;WITH cte AS
(
    SELECT TOP (@BatchSize) *
    FROM OutboxMessages WITH
    (
        UPDLOCK,
        READPAST,
        ROWLOCK
    )
    WHERE
        (
            Status = @Pending
            OR
            (
                Status = @Processing
                AND LockedUntilUtc < @Now
            )
        )
        AND
        (
            NextAttemptOnUtc IS NULL
            OR NextAttemptOnUtc <= @Now
        )
    ORDER BY CreatedOnUtc
)

UPDATE cte
SET
    Status = @Processing,
    LockId = @LockId,
    LockedUntilUtc = @LockedUntilUtc

OUTPUT INSERTED.*;