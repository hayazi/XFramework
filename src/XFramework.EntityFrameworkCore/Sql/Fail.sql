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