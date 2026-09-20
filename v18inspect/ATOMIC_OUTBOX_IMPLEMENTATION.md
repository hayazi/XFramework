# Atomic Outbox Claim

The outbox claim operation is now implemented as a single SQL Server `UPDATE ... OUTPUT` statement.

## Concurrency behavior

Multiple workers can execute `ClaimBatchAsync` concurrently. `UPDLOCK` and `READPAST` ensure that rows locked by another worker are skipped while the claiming worker atomically changes the selected rows to `Processing` and assigns its lease (`LockId` + `LockedUntilUtc`).

`READCOMMITTEDLOCK` keeps the locking semantics explicit when SQL Server's `READ_COMMITTED_SNAPSHOT` database option is enabled.

## Claim states

```text
Pending ---------------------> Processing
                                  |
                                  | lease expires
                                  v
                              Processing
                                  |
                                  v
                                Pending
```

An already-processing message is claimable only after its lease expires.

## Important guarantee

The claim itself is atomic. This prevents two workers from successfully claiming the same row at the same time under normal SQL Server locking semantics.

Idempotency is still required because a worker can crash after publishing an event and before marking the outbox message completed.
