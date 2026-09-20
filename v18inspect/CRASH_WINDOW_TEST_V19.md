# Reliability Test v19 — Commit Before ACK Crash Window

This test verifies the critical RabbitMQ crash window:

1. Publish one event.
2. First delivery is processed and the application transaction commits.
3. The consumer channel is deliberately closed before `BasicAckAsync`.
4. RabbitMQ requeues the unacknowledged message.
5. A second consumer receives the message with `Redelivered = true`.
6. `EventProcessor` sees the same `EventId` through idempotency.
7. The business handler is not executed a second time.
8. The second delivery is acknowledged successfully.

Expected business effect:

- `SaveChangesAsync` = 1
- transaction commit = 1
- retry = 0
- DLQ = 0
- second delivery = broker redelivery

This is a real RabbitMQ redelivery test; it does not simply publish the same event twice.
