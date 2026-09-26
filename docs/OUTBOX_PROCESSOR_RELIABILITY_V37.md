# Outbox Processor Reliability Tests - V37

## Fixes

Two test assertions from V36 were too strict:

1. `Cancellation_during_publish_should_propagate_without_marking_retry_or_failure`
   - `TaskCanceledException` derives from `OperationCanceledException`.
   - The test now uses `Assert.ThrowsAnyAsync<OperationCanceledException>`.
   - Production behavior remains unchanged: cancellation is rethrown and the claimed message is not marked Retry/Failed.

2. `Retryable_publish_failure_should_mark_message_for_retry`
   - `OutboxProcessor` stores `exception.ToString()` in `LastError`, which intentionally preserves exception type and diagnostic details.
   - The test now verifies that the stored error contains the expected `RabbitMQ timeout.` message instead of requiring an exact string match.

No production code was changed in V37.
