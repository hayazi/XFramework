# XFramework Integration Tests v26

## Fix: Atomic Outbox Interceptor

v25 exposed a real reliability-test defect: the atomic persistence test committed the aggregate but produced no OutboxMessage.

The interceptor has been hardened in v26:

- explicitly calls `ChangeTracker.DetectChanges()` before scanning domain-event entries;
- supports both synchronous and asynchronous `SaveChanges` interception;
- does not clear domain events before persistence succeeds;
- clears pending domain events only from `SavedChanges` / `SavedChangesAsync`;
- retains domain events when `SaveChanges` fails;
- prevents duplicate Outbox rows when the same DbContext invokes `SaveChanges` more than once for the same event.

The test suite adds a failure-path test proving that a failed SaveChanges does not silently lose the domain event.

Expected result:

```text
XFramework.IntegrationTests: 12 passed, 0 failed
Build succeeded
```
