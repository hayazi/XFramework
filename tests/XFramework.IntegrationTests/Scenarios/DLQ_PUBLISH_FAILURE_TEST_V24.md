# DLQ Publish Failure Test - v24

This test verifies the critical consumer invariant:

1. The original event is delivered.
2. Business processing fails with a non-retryable exception.
3. DLQ publication fails.
4. The original message is NOT acknowledged.
5. The consumer connection is closed to simulate a crash.
6. RabbitMQ redelivers the original message with `Redelivered = true`.
7. On the second delivery the DLQ publisher is healthy.
8. The event is published to the DLQ successfully.
9. The original delivery is then acknowledged.

The first delivery expects the DLQ publish exception to propagate. The second
redelivery must complete successfully; it must NOT expect another exception.
