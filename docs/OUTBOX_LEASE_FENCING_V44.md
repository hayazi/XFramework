# Outbox Lease Fencing — V44

## Baseline

V44 continues from the user-verified XFramework baseline.

- XFramework.Tests: 10/10
- XFramework.IntegrationTests: 24/24
- .NET 10
- SQL Server persistence for the Outbox

## Problem

An Outbox worker owns a message through its `LockId` and a time-bounded lease.
When the lease expires, another worker is allowed to reclaim the message with a new `LockId`.

A stale worker must not be able to complete, retry, or fail the message after its lease has expired or after another worker has reclaimed it.

Without lease fencing, the following race is possible:

1. Worker A claims message with `LockId=A`.
2. Worker A stops making progress.
3. Lease expires.
4. Worker B reclaims the message with `LockId=B`.
5. Worker A resumes and attempts `MarkCompleted`, `MarkRetry`, or `MarkFailed`.
6. A stale completion could overwrite Worker B's state if persistence only checks the old lock identity.

## V44 rule

A state transition from `Processing` is valid only when all of these are true:

```text
Id matches
AND
LockId matches
AND
Status = Processing
AND
LockedUntilUtc > NowUtc
```

This is the Outbox lease-fencing invariant.

## Production changes

`RenewLeaseAsync`, `MarkCompletedAsync`, `MarkRetryAsync`, and `MarkFailedAsync` now require the current UTC time and verify that the lease is still valid.

The SQL update therefore cannot succeed for an expired worker even when its `LockId` still matches the row.

The processor captures a single timestamp immediately before each state transition and passes it to the repository.

## Integration tests

`OutboxLeaseFencingSqlServerTests` contains two opt-in SQL Server scenarios:

1. An expired worker attempts completion before reclaim. The row remains `Processing` with the original lock.
2. Worker B reclaims the message with a new lock. Stale Worker A attempts completion, retry, and failure; none can modify Worker B's message. Worker B can then complete normally.

The tests use:

```text
XFRAMEWORK_SQLSERVER_TEST_CONNECTION
```

When the environment variable is not configured, the tests return immediately, consistent with the existing SQL Server concurrency test convention in this project.
