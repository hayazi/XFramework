# Retry Integration Test v14

Adds an integration test for the real RabbitMQ retry topology.

Scenario:

1. Publish an EventEnvelope to the main exchange.
2. Consumer receives it with RetryCount = 0.
3. Test processor throws TimeoutException.
4. RabbitMqMessageHandler uses the real RabbitMqRetryPublisher.
5. RetryPublisher increments RetryCount and publishes to the retry exchange.
6. The retry queue holds the message for 5 seconds.
7. RabbitMQ dead-letters the message back to the main exchange.
8. Consumer receives the same EventId with RetryCount = 1.
9. Processor succeeds.
10. No DLQ path is used.

This test validates the RabbitMQ retry transport itself, not just the retry policy in isolation.
