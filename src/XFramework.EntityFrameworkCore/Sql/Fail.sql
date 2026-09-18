-- Retry a leased Outbox message.
UPDATE OutboxMessages
SET
    Status = @Pending,
    RetryCount = RetryCount + 1,
    NextAttemptOnUtc = @NextAttemptOnUtc,
    LastError = @LastError,
    LockId = NULL,
    LockedUntilUtc = NULL
WHERE Id = @Id
  AND LockId = @LockId;

-- Terminal failure after the retry policy is exhausted.
UPDATE OutboxMessages
SET
    Status = @Failed,
    RetryCount = RetryCount + 1,
    LastError = @LastError,
    LockId = NULL,
    LockedUntilUtc = NULL
WHERE Id = @Id
  AND LockId = @LockId;
