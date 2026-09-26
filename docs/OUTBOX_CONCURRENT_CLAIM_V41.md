# Outbox Concurrent Claim Reliability - V41

Added a SQL Server integration test proving that two workers concurrently calling `ClaimBatchAsync` cannot claim the same pending Outbox message.

The test is opt-in through `XFRAMEWORK_SQLSERVER_TEST_CONNECTION` because the integration test suite historically uses SQLite and RabbitMQ fixtures.

Expected invariant: exactly one worker receives the message; the other receives an empty batch.

This validates the SQL Server `UPDLOCK + READPAST + ROWLOCK + READCOMMITTEDLOCK` atomic claim strategy.
