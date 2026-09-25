# Outbox Transition Result Handling & Observability — V48

## Purpose

V48 makes ownership-sensitive outbox state transitions operationally visible.

The repository already returns `bool` from `RenewLeaseAsync`, `MarkCompletedAsync`, `MarkRetryAsync`, and `MarkFailedAsync`. V48 makes `OutboxProcessor` consume those results explicitly and records ownership-loss diagnostics.

## Ownership-sensitive transitions

A transition is successful only when the SQL `UPDATE` still owns the row through:

- `Id`
- `LockId`
- `Status = Processing`
- `LockedUntilUtc > now`

An affected-row count of zero therefore means that this worker no longer owns the message.

## Processor behavior

### Lease renewal lost

The processor logs a warning and does not publish the message.

### Completion ownership lost after publish

The processor logs a warning and does not publish again locally. A later worker may publish the same `EventId`.

This preserves:

- at-least-once delivery
- no unsafe local duplicate publish after a lost completion
- idempotent consumer semantics

### Retry ownership lost

The processor logs a warning. It does not attempt another retry-state update with the stale lock.

### Failure ownership lost

The processor logs a warning. It does not attempt another failure-state update with the stale lock.

## Metrics

`OutboxDiagnostics` exposes an OpenTelemetry-compatible `Meter` named:

`XFramework.Outbox`

Counters:

- `xframework.outbox.published`
- `xframework.outbox.completed`
- `xframework.outbox.retried`
- `xframework.outbox.failed`
- `xframework.outbox.lease_renewal_lost`
- `xframework.outbox.completion_ownership_lost`
- `xframework.outbox.retry_ownership_lost`
- `xframework.outbox.failure_ownership_lost`

The framework does not select an exporter. The host application can connect the meter to OpenTelemetry, Prometheus, or another metrics backend.

## Important delivery guarantee

The transactional outbox remains an **at-least-once** delivery mechanism. SQL Server and RabbitMQ are not coordinated by a distributed transaction.

Exactly-once business effect is achieved by making downstream event handlers idempotent, not by pretending that publication itself is exactly once.
