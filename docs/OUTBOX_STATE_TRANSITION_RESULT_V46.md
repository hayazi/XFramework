# Outbox State Transition Result Semantics — V46

## Purpose

V46 makes ownership-sensitive Outbox state transitions return whether the SQL update actually changed exactly one row. This exposes lease-fencing failures to the processor instead of silently hiding them.

## Changes

`IOutboxRepository` now returns `Task<bool>` from:

- `MarkCompletedAsync`
- `MarkRetryAsync`
- `MarkFailedAsync`

The EF Core implementation returns `true` only when the guarded `UPDATE` affects exactly one row. The guards remain:

```text
Id matches
AND LockId matches
AND Status = Processing
AND LockedUntilUtc > NowUtc
```

## Processor behavior

If publish succeeds but completion returns `false`, the event has already been published and this worker has lost ownership. The processor therefore does not publish the event again as a local retry. The same EventId may be published by a later worker after reclaim. Downstream event handlers must remain idempotent.

## Why this matters

A void state-transition method can hide a lost lease. V46 makes the result explicit and testable:

```text
Publish succeeds
      ↓
MarkCompletedAsync → false
      ↓
worker no longer owns row
      ↓
no local re-publish
      ↓
future worker may publish same EventId
      ↓
idempotent consumer protects business side effects
```

This preserves the intended at-least-once delivery model while making stale-worker behavior observable at the repository boundary.
