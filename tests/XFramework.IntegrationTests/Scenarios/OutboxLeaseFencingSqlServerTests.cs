using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;
using XFramework.EntityFrameworkCore.Outbox;
using XFramework.EntityFrameworkCore.Persistence;
using Xunit;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class OutboxLeaseFencingSqlServerTests
{
    [Fact]
    public async Task Expired_worker_must_not_complete_message_before_reclaim()
    {
        var connectionString = Environment.GetEnvironmentVariable("XFRAMEWORK_SQLSERVER_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            return; // Set XFRAMEWORK_SQLSERVER_TEST_CONNECTION to run SQL Server lease-fencing tests.

        var registry = new EventTypeRegistry();
        var messageId = Guid.NewGuid();
        var eventType = "Integration.LeaseFencing.BeforeReclaim";
        var workerALock = Guid.NewGuid().ToString("N");
        var claimTime = DateTime.UtcNow;
        var expiredLease = claimTime.AddSeconds(30);
        var nowAfterExpiry = expiredLease.AddSeconds(1);

        try
        {
            await using (var setup = CreateContext(connectionString, registry))
            {
                await setup.Database.EnsureCreatedAsync();
                await setup.Database.ExecuteSqlRawAsync(
                    "DELETE FROM OutboxMessages WHERE EventType = 'Integration.LeaseFencing.BeforeReclaim';");

                setup.OutboxMessages.Add(new OutboxMessage
                {
                    Id = messageId,
                    EventId = Guid.NewGuid(),
                    EventType = eventType,
                    EventVersion = 1,
                    Payload = "{}",
                    Status = OutboxMessageStatus.Processing,
                    RetryCount = 0,
                    CreatedOnUtc = claimTime.AddMinutes(-2),
                    OccurredOnUtc = claimTime.AddMinutes(-2),
                    LockId = workerALock,
                    LockedUntilUtc = expiredLease
                });

                await setup.SaveChangesAsync();
            }

            await using var workerAContext = CreateContext(connectionString, registry);
            var workerARepository = new OutboxRepository(workerAContext);

            var completed = await workerARepository.MarkCompletedAsync(
                messageId,
                workerALock,
                nowAfterExpiry,
                nowAfterExpiry);

            Assert.False(completed);

            await using var verification = CreateContext(connectionString, registry);
            var row = await verification.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == messageId);

            Assert.Equal(OutboxMessageStatus.Processing, row.Status);
            Assert.Equal(workerALock, row.LockId);
            Assert.Equal(expiredLease, row.LockedUntilUtc);
            Assert.Null(row.ProcessedOnUtc);
        }
        finally
        {
            await using var cleanup = CreateContext(connectionString, registry);
            await cleanup.Database.ExecuteSqlRawAsync(
                "DELETE FROM OutboxMessages WHERE EventType = 'Integration.LeaseFencing.BeforeReclaim';");
        }
    }

    [Fact]
    public async Task Stale_worker_must_not_overwrite_reclaimed_message()
    {
        var connectionString = Environment.GetEnvironmentVariable("XFRAMEWORK_SQLSERVER_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            return; // Set XFRAMEWORK_SQLSERVER_TEST_CONNECTION to run SQL Server lease-fencing tests.

        var registry = new EventTypeRegistry();
        var messageId = Guid.NewGuid();
        var eventType = "Integration.LeaseFencing.AfterReclaim";
        var workerALock = Guid.NewGuid().ToString("N");
        var workerBLock = Guid.NewGuid().ToString("N");
        var claimTime = DateTime.UtcNow;
        var expiredLease = claimTime.AddSeconds(30);
        var reclaimTime = expiredLease.AddSeconds(1);
        var workerBLease = reclaimTime.AddMinutes(2);

        try
        {
            await using (var setup = CreateContext(connectionString, registry))
            {
                await setup.Database.EnsureCreatedAsync();
                await setup.Database.ExecuteSqlRawAsync(
                    "DELETE FROM OutboxMessages WHERE EventType = 'Integration.LeaseFencing.AfterReclaim';");

                setup.OutboxMessages.Add(new OutboxMessage
                {
                    Id = messageId,
                    EventId = Guid.NewGuid(),
                    EventType = eventType,
                    EventVersion = 1,
                    Payload = "{}",
                    Status = OutboxMessageStatus.Processing,
                    RetryCount = 0,
                    CreatedOnUtc = claimTime.AddMinutes(-2),
                    OccurredOnUtc = claimTime.AddMinutes(-2),
                    LockId = workerALock,
                    LockedUntilUtc = expiredLease
                });

                await setup.SaveChangesAsync();
            }

            await using var workerBContext = CreateContext(connectionString, registry);
            var workerBRepository = new OutboxRepository(workerBContext);

            await workerBRepository.ReleaseExpiredLeasesAsync(reclaimTime);
            var claimed = await workerBRepository.ClaimBatchAsync(
                1,
                workerBLock,
                reclaimTime,
                workerBLease);

            var claimedMessage = Assert.Single(claimed);
            Assert.Equal(messageId, claimedMessage.Id);
            Assert.Equal(workerBLock, claimedMessage.LockId);

            await using var workerAContext = CreateContext(connectionString, registry);
            var workerARepository = new OutboxRepository(workerAContext);

            // Worker A is stale. Its old fencing identity must not be able to
            // complete, retry, or fail Worker B's reclaimed message.
            var staleCompleted = await workerARepository.MarkCompletedAsync(
                messageId,
                workerALock,
                reclaimTime.AddSeconds(1),
                reclaimTime.AddSeconds(1));
            Assert.False(staleCompleted);

            var staleRetried = await workerARepository.MarkRetryAsync(
                messageId,
                workerALock,
                reclaimTime.AddSeconds(2),
                reclaimTime.AddMinutes(1),
                "stale worker retry");
            Assert.False(staleRetried);

            var staleFailed = await workerARepository.MarkFailedAsync(
                messageId,
                workerALock,
                reclaimTime.AddSeconds(3),
                "stale worker failure");
            Assert.False(staleFailed);

            await using var verification = CreateContext(connectionString, registry);
            var row = await verification.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == messageId);

            Assert.Equal(OutboxMessageStatus.Processing, row.Status);
            Assert.Equal(workerBLock, row.LockId);
            Assert.Equal(workerBLease, row.LockedUntilUtc);
            Assert.Equal(0, row.RetryCount);
            Assert.Null(row.ProcessedOnUtc);
            Assert.Null(row.LastError);

            await workerBRepository.MarkCompletedAsync(
                messageId,
                workerBLock,
                reclaimTime.AddSeconds(4),
                reclaimTime.AddSeconds(4));

            var completed = await verification.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == messageId);

            Assert.Equal(OutboxMessageStatus.Completed, completed.Status);
            Assert.Null(completed.LockId);
            Assert.Null(completed.LockedUntilUtc);
            Assert.NotNull(completed.ProcessedOnUtc);
        }
        finally
        {
            await using var cleanup = CreateContext(connectionString, registry);
            await cleanup.Database.ExecuteSqlRawAsync(
                "DELETE FROM OutboxMessages WHERE EventType = 'Integration.LeaseFencing.AfterReclaim';");
        }
    }

    private static XFrameworkDbContext CreateContext(
        string connectionString,
        EventTypeRegistry registry)
    {
        var options = new DbContextOptionsBuilder<XFrameworkDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new XFrameworkDbContext(options, registry);
    }
}
