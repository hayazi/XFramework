# Outbox Architecture and Reliability Contract

## Purpose
Commit business state and an Outbox event record in the same SQL Server transaction; publish asynchronously to RabbitMQ so a process crash between DB commit and broker publish does not silently lose the event.

## Delivery semantics
**At-least-once**, not exactly-once. The publisher can successfully send to RabbitMQ and then fail to mark the Outbox row complete, or the process can crash before ACK/commit bookkeeping. The row may be reclaimed and published again. Consumers must be idempotent.

## Lifecycle
```text
Business transaction
  -> Domain event captured
  -> Outbox row persisted in same DB transaction
  -> Worker claims row with lock/lease
  -> Event envelope serialized
  -> Event bus publish
  -> Mark completed (ownership-fenced)
```
Failure path:
```text
Publish fails
  -> retry policy / next-attempt time
  -> retry while allowed
  -> terminal Failed / DLQ path after limit
```

## Ownership and lease invariants
- Claiming is atomic and must prevent two active workers from owning the same unexpired row.
- Lease expiration enables reclaim.
- Renew/completion/retry/failure must verify current LockId and Processing state; lease-time fencing prevents expired owners from updating state.
- A stale worker must not overwrite a new owner's state.
- A state transition affecting zero rows means the caller did not successfully transition the row (typically ownership lost).

## Crash windows
1. Crash before publish: lease expires and another worker can publish.
2. Crash after publish but before completion: duplicate publication is possible.
3. Publish success + completion false: old worker must not locally republish; a later worker may publish again.
4. Retry/DLQ publish failure: do not ACK/mark success in a way that loses the original event; preserve broker/outbox recovery semantics.

## Consumer idempotency
Use `(EventId, HandlerName)` as the deduplication key. The idempotency marker and handler's business side effects must share one transaction. A duplicate should not repeat the business effect.

## Operational guidance
- Track pending age, attempts, retries, terminal failures, lease-renewal loss, and ownership-loss outcomes.
- Avoid high-cardinality metric tags such as EventId, aggregate ID, or user ID.
- Preserve EventId, CorrelationId, and CausationId in structured logs and message headers.
- Admin replay must be audited and must preserve idempotency semantics.
