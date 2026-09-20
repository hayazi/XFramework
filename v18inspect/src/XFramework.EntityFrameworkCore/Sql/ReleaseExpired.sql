UPDATE OutboxMessages
SET
    Status = @Pending,

    LockId = NULL,

    LockedUntilUtc = NULL
WHERE
    Status = @Processing
    AND LockedUntilUtc < @Now