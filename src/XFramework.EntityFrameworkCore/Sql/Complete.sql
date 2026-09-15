UPDATE OutboxMessages
SET
    Status = @Completed,

    ProcessedOnUtc = @Now,

    LockId = NULL,

    LockedUntilUtc = NULL
WHERE
    Id = @Id
    AND LockId = @LockId