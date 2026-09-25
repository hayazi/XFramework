# Outbox Publish Ownership — V47

## Purpose

V47 makes the delivery guarantee around the SQL Server Outbox and the external event bus explicit:

- Publishing an event and completing its Outbox row are two separate operations.
- A worker can successfully publish an event and then lose ownership of the Outbox row before `MarkCompletedAsync`.
- When that happens, the current worker must **not** publish the event again locally.
- A later worker may reclaim the row and publish the same `EventId` again.
- Therefore the Outbox boundary provides **at-least-once delivery**, not exactly-once publication.
- Consumers must use the existing EventId + handler idempotency mechanism so duplicate delivery does not duplicate business side effects.

## Scenario

```text
Worker A claims EventId E
        |
        v
   Publish(E) -----> RabbitMQ
        |
        | lease expires / ownership is lost
        v
MarkCompleted(A) = false
        |
        v
Worker B reclaims the same Outbox row
        |
        v
   Publish(E) -----> RabbitMQ again
        |
        v
Consumer receives E twice
        |
        v
Idempotency: first delivery = process
             second delivery = ignore
```

## Important invariant

The framework deliberately does not attempt distributed exactly-once semantics between SQL Server and RabbitMQ.

The intended model is:

```text
SQL Outbox
    + fenced ownership
    + crash recovery
    + at-least-once publish
            |
            v
RabbitMQ
            |
            v
Idempotent Consumer
    EventId + HandlerName
            |
            v
Exactly-once business effect
```

The final line means the business side effect is applied once by the consumer's idempotency boundary; it does **not** mean RabbitMQ receives the event only once.

## V47 tests

`OutboxPublishOwnershipTests` verifies:

1. If worker A publishes successfully but loses ownership before completion, the same `EventId` can be published by the next worker.
2. The worker that loses ownership does not immediately re-publish the already-published event.
3. The next worker can complete the reclaimed Outbox message.
4. No local retry/failure transition is generated solely because ownership was lost after a successful publish.

Existing RabbitMQ crash-window and duplicate-delivery tests continue to verify the downstream idempotency boundary.
