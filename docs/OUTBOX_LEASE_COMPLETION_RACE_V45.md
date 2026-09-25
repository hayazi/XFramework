# Outbox Lease Completion Race — V45

## Goal

V45 verifies the final ownership boundary of the Outbox lease after V44 lease fencing:

```text
Worker A claims Lock A
        ↓
Lease A expires
        ↓
Worker B reclaims the message with Lock B
        ↓
Worker A resumes with stale Lock A
        ↓
Worker A must NOT complete/retry/fail Worker B's message
```

## Production invariant

Every state-changing operation performed by a worker is fenced by:

```text
Id matches
AND LockId matches
AND Status = Processing
AND LockedUntilUtc > NowUtc
```

This is implemented by the repository SQL for:

- `RenewLeaseAsync`
- `MarkCompletedAsync`
- `MarkRetryAsync`
- `MarkFailedAsync`

## V45 tests

### 1. Concurrent completion after reclaim

Worker B first reclaims the expired message and obtains Lock B. Worker A then races `MarkCompletedAsync` using stale Lock A against Worker B's valid completion.

Expected result:

- final status = `Completed`
- `LockId` = `NULL`
- `LockedUntilUtc` = `NULL`
- stale Worker A cannot overwrite Worker B

### 2. Expired completion vs lease release

Worker A's lease has already expired. `MarkCompletedAsync` and `ReleaseExpiredLeasesAsync` are deliberately started concurrently.

Expected result:

- final status = `Pending`
- `LockId` = `NULL`
- `LockedUntilUtc` = `NULL`
- Worker A cannot complete an already-expired lease

## Test environment

The SQL Server race tests are opt-in:

```powershell
$env:XFRAMEWORK_SQLSERVER_TEST_CONNECTION = "<SQL Server connection string>"
```

Without this environment variable the tests return immediately, preserving the existing opt-in behavior used by the SQL Server integration scenarios.

## Why V45 matters

RabbitMQ and the Outbox processor are distributed systems. A worker can pause after its lease expires and resume later. Without fencing, that stale worker could mutate a message that has already been reclaimed by another worker.

V45 verifies that the database itself enforces ownership at the state-transition boundary.
