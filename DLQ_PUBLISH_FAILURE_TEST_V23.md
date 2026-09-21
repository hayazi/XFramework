# XFramework Integration Tests v23 — DLQ Publish Failure

## Purpose

This test verifies the consumer safety invariant for non-retryable failures:

1. RabbitMQ delivers an event.
2. Business processing fails with a non-retryable exception.
3. DLQ publication is attempted and fails.
4. The original message must NOT be acknowledged.
5. The consumer connection is closed to simulate a crash.
6. RabbitMQ redelivers the original message with `Redelivered = true`.
7. The second delivery attempts DLQ publication again.
8. The successful DLQ publication allows the normal handler path to ACK the original message.

## Safety invariant

A consumer must never ACK the original message after a failed DLQ publication. Otherwise the event could be lost.

## Expected result

The complete integration suite should report:

- 9 passed
- 0 failed
- Build succeeded
