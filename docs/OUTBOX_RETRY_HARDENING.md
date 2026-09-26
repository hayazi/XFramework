# Outbox Retry / Dead-Letter Hardening

## Goals

- Keep transient broker/network failures retryable.
- Make `Failed` a terminal state after the retry policy is exhausted.
- Never schedule a permanent failure for another polling cycle.
- Preserve the last error for operator diagnostics.
- Keep completion/retry/failure updates lease-owned by `LockId`.

## State model

```text
Pending -> Processing -> Completed
                    |
                    +-> Pending (transient failure)
                    |
                    +-> Failed (terminal failure)
```

`Failed` is intentionally excluded from the atomic claim query.

## Retry schedule

```text
5s -> 30s -> 2m -> 10m -> 30m -> Failed
```

The retry count is zero-based when selecting a delay. Once the policy returns
`false`, the processor calls `MarkFailedAsync` and the message becomes terminal.

## Operational recovery

A failed message should be inspected and explicitly replayed by an operator or
administrative application service after the root cause is fixed. Automatic
requeue is deliberately not performed because that can create an endless retry
loop.

## Important distinction

The Outbox is the durable handoff from the database transaction to the message
broker. A terminal Outbox failure means the handoff could not be completed; it
does not mean the original business transaction should be rolled back after its
commit.
