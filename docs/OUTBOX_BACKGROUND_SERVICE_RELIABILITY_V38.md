# Outbox Background Service Reliability - V38

V38 adds integration tests for the existing `OutboxBackgroundService` without changing production behavior.

Scenarios covered:

1. Worker starts, creates a scope, invokes `IOutboxProcessor`, and stops cleanly on shutdown.
2. A transient processor exception is logged by the worker and the worker continues polling.
3. Cancellation observed inside the processor propagates as shutdown and the worker exits cleanly.

Production code was intentionally unchanged in V38.
