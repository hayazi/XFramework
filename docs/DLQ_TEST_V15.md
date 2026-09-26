# XFramework Integration Tests v15

Added a real RabbitMQ DLQ integration test.

Scenario:

Main Exchange -> Main Queue -> RabbitMqMessageHandler -> non-retryable exception -> Dead Letter Exchange -> DLQ

Assertions:
- Event is processed once.
- Non-retryable exception is not retried.
- Original delivery is acknowledged after DLQ publish succeeds.
- DLQ contains the same EventId/EventType.
- RetryCount remains 0.
- LastError contains the processing exception.
