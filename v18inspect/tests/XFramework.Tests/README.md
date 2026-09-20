# XFramework Tests

This project contains fast, deterministic tests for framework messaging contracts and retry behavior.

## Current coverage

- RabbitMQ exchange/queue/routing-key naming
- Retry delay mapping
- Event retry policy classification and retry limits
- RabbitMQ default configuration

These tests do not require a running RabbitMQ or SQL Server instance.

## Next integration layer

When the foundation tests are green, add environment-backed integration tests for:

1. publish confirms
2. consumer ACK behavior
3. retry queue TTL/dead-letter flow
4. DLQ publishing
5. RabbitMQ restart/recovery
6. concurrent consumers
7. SQL Server outbox claim concurrency
8. idempotent event processing
