# XFramework Reliability Tests v13

## Added scenario

`RabbitMqEventBusIdempotencyTests.Duplicate_delivery_should_be_processed_only_once`

The test publishes the exact same `EventEnvelope` twice with the same `EventId`.
Both RabbitMQ deliveries must be acknowledged successfully, while the application
handler and unit-of-work commit execute only once.

Expected behavior:

```text
Event #1
  -> RabbitMQ
  -> EventProcessor
  -> Idempotency: first processing = true
  -> Handler
  -> SaveChanges
  -> Commit
  -> ACK

Event #2 (same EventId)
  -> RabbitMQ
  -> EventProcessor
  -> Idempotency: first processing = false
  -> Rollback transaction
  -> no handler
  -> no SaveChanges
  -> no Commit
  -> ACK
```

This validates the crash-window protection between business processing and broker acknowledgement.
