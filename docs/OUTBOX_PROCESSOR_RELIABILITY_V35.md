# Outbox Processor Reliability — v35

This version is based on the user's verified XFramework baseline.

## Verified baseline supplied by the user

- `XFramework.Tests`: 10 passed, 0 failed.
- `XFramework.IntegrationTests`: 17 passed, 0 failed.
- Target framework: .NET 10.

## Changes

### Cancellation handling

`OutboxProcessor` now treats an `OperationCanceledException` caused by the processor's cancellation token as worker shutdown/cancellation rather than a publish failure.

The claimed message is intentionally left in `Processing`. Its lease can expire and the existing lease-recovery mechanism can make it available again.

### Processor reliability tests

Four tests cover:

1. Successful publish → `Completed`.
2. Retryable publish failure → `Pending` with retry schedule and error.
3. Non-retryable publish failure → `Failed` with error.
4. Cancellation during publish → cancellation propagates and no retry/failure state transition is attempted.

These tests use an in-memory fake repository/event bus so they verify `OutboxProcessor` state-transition behavior without requiring RabbitMQ or SQL Server.

## Next reliability boundary

After these processor-level tests pass, the next step is to harden the production outbox worker around lease ownership, concurrent processors, and stale-worker protection. In particular, completion/retry/failure updates should be verified to affect only the message currently owned by the processor's `LockId`.
