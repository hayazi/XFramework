using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;
using XFramework.EntityFrameworkCore.Outbox;
using XFramework.EntityFrameworkCore.Persistence;
using Xunit;

namespace XFramework.IntegrationTests.Scenarios;

/// <summary>
/// V45 verifies that an expired worker cannot win a completion race after
/// another worker has reclaimed the same outbox message.
///
/// These tests require XFRAMEWORK_SQLSERVER_TEST_CONNECTION because the
/// guarantee depends on real SQL Server row-update semantics.
/// </summary>
public sealed class OutboxLeaseCompletionRaceSqlServerTests
{
    [Fact]
    public async Task Stale_completion_must_lose_concurrent_completion_race_after_reclaim()
    {
        var connectionString = Environment.GetEnvironmentVariable("XFRAMEWORK_SQLSERVER_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            return; // Set XFRAMEWORK_SQLSERVER_TEST_CONNECTION to run SQL Server race tests.

        var registry = new EventTypeRegistry();
        var messageId = Guid.NewGuid();
        var eventType = "Integration.LeaseRace.ConcurrentCompletion";
        var workerALock = Guid.NewGuid().ToString("N");
        var workerBLock = Guid.NewGuid().ToString("N");
        var claimTime = DateTime.UtcNow;
        var expiredLease = claimTime.AddSeconds(30);
        var reclaimTime = expiredLease.AddSeconds(1);
        var workerBLease = reclaimTime.AddMinutes(2);
        var raceTime = reclaimTime.AddSeconds(1);

        try
        {
            await using (var setup = CreateContext(connectionString, registry))
            {
                await setup.Database.EnsureCreatedAsync();
                await setup.Database.ExecuteSqlRawAsync(
                    "DELETE FROM OutboxMessages WHERE EventType = 'Integration.LeaseRace.ConcurrentCompletion';");

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

            // Worker B performs the reclaim first. From this point on, Lock A
            // is stale and must never be able to modify the row.
            await using var workerBContext = CreateContext(connectionString, registry);
            var workerBRepository = new OutboxRepository(workerBContext);

            await workerBRepository.ReleaseExpiredLeasesAsync(reclaimTime);
            var claimed = await workerBRepository.ClaimBatchAsync(
                1,
                workerBLock,
                reclaimTime,
                workerBLease);

            Assert.Single(claimed);
            Assert.Equal(workerBLock, claimed[0].LockId);

            await using var workerAContext = CreateContext(connectionString, registry);
            var workerARepository = new OutboxRepository(workerAContext);

            // The operations are intentionally started concurrently. SQL Server
            // must still fence Worker A by LockId even while Worker B completes.
            var staleCompletion = workerARepository.MarkCompletedAsync(
                messageId,
                workerALock,
                raceTime,
                raceTime);

            var validCompletion = workerBRepository.MarkCompletedAsync(
                messageId,
                workerBLock,
                raceTime,
                raceTime);

            await Task.WhenAll(staleCompletion, validCompletion);

            Assert.False(await staleCompletion);
            Assert.True(await validCompletion);

            await using var verification = CreateContext(connectionString, registry);
            var row = await verification.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == messageId);

            Assert.Equal(OutboxMessageStatus.Completed, row.Status);
            Assert.Null(row.LockId);
            Assert.Null(row.LockedUntilUtc);
            Assert.NotNull(row.ProcessedOnUtc);
            Assert.Null(row.LastError);
        }
        finally
        {
            await using var cleanup = CreateContext(connectionString, registry);
            await cleanup.Database.ExecuteSqlRawAsync(
                "DELETE FROM OutboxMessages WHERE EventType = 'Integration.LeaseRace.ConcurrentCompletion';");
        }
    }

    [Fact]
    public async Task Expired_worker_completion_must_lose_race_with_lease_reclamation()
    {
        var connectionString = Environment.GetEnvironmentVariable("XFRAMEWORK_SQLSERVER_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            return; // Set XFRAMEWORK_SQLSERVER_TEST_CONNECTION to run SQL Server race tests.

        var registry = new EventTypeRegistry();
        var messageId = Guid.NewGuid();
        var eventType = "Integration.LeaseRace.ReleaseAndComplete";
        var workerALock = Guid.NewGuid().ToString("N");
        var claimTime = DateTime.UtcNow;
        var expiredLease = claimTime.AddSeconds(30);
        var reclaimTime = expiredLease.AddSeconds(1);

        try
        {
            await using (var setup = CreateContext(connectionString, registry))
            {
                await setup.Database.EnsureCreatedAsync();
                await setup.Database.ExecuteSqlRawAsync(
                    "DELETE FROM OutboxMessages WHERE EventType = 'Integration.LeaseRace.ReleaseAndComplete';");

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
            await using var workerBContext = CreateContext(connectionString, registry);
            var workerARepository = new OutboxRepository(workerAContext);
            var workerBRepository = new OutboxRepository(workerBContext);

            // This is the boundary where Worker A has lost its lease. The two
            // SQL operations are raced deliberately; A must not complete because
            // its lease is already expired, while B releases the lease.
            var staleCompletion = workerARepository.MarkCompletedAsync(
                messageId,
                workerALock,
                reclaimTime,
                reclaimTime);

            var release = workerBRepository.ReleaseExpiredLeasesAsync(reclaimTime);

            await Task.WhenAll(staleCompletion, release);

            Assert.False(await staleCompletion);

            await using var verification = CreateContext(connectionString, registry);
            var row = await verification.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == messageId);

            Assert.Equal(OutboxMessageStatus.Pending, row.Status);
            Assert.Null(row.LockId);
            Assert.Null(row.LockedUntilUtc);
            Assert.Null(row.ProcessedOnUtc);
        }
        finally
        {
            await using var cleanup = CreateContext(connectionString, registry);
            await cleanup.Database.ExecuteSqlRawAsync(
                "DELETE FROM OutboxMessages WHERE EventType = 'Integration.LeaseRace.ReleaseAndComplete';");
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
