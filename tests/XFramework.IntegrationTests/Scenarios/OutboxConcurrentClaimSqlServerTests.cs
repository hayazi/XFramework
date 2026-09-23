using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;
using XFramework.EntityFrameworkCore.Outbox;
using XFramework.EntityFrameworkCore.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class OutboxConcurrentClaimSqlServerTests
{
    [Fact]
    public async Task Two_workers_must_not_claim_the_same_pending_message()
    {
        var connectionString = Environment.GetEnvironmentVariable("XFRAMEWORK_SQLSERVER_TEST_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return; // Skip: Set XFRAMEWORK_SQLSERVER_TEST_CONNECTION to run SQL Server concurrency tests.
        }

        var registry = new EventTypeRegistry();
        await using (var setup = CreateContext(connectionString!, registry))
        {
            await setup.Database.EnsureCreatedAsync();

            await setup.Database.ExecuteSqlRawAsync("DELETE FROM OutboxMessages;");

            setup.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventType = "Integration.ConcurrentClaim",
                EventVersion = 1,
                Payload = "{}",
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                CreatedOnUtc = DateTime.UtcNow,
                OccurredOnUtc = DateTime.UtcNow
            });

            await setup.SaveChangesAsync();
        }

        var now = DateTime.UtcNow;
        var lockUntil = now.AddMinutes(2);
        var lockA = Guid.NewGuid().ToString("N");
        var lockB = Guid.NewGuid().ToString("N");

        await using var contextA = CreateContext(connectionString!, registry);
        await using var contextB = CreateContext(connectionString!, registry);

        var repositoryA = new OutboxRepository(contextA);
        var repositoryB = new OutboxRepository(contextB);

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var taskA = ClaimAfterStartAsync(repositoryA, lockA, now, lockUntil, start.Task);
        var taskB = ClaimAfterStartAsync(repositoryB, lockB, now, lockUntil, start.Task);

        start.SetResult();

        var results = await Task.WhenAll(taskA, taskB);
        var claimed = results.SelectMany(x => x).ToList();

        Assert.Single(claimed);
        Assert.True(claimed[0].LockId == lockA || claimed[0].LockId == lockB);

        await using var verification = CreateContext(connectionString!, registry);
        var rows = await verification.OutboxMessages
            .AsNoTracking()
            .Where(x => x.EventType == "Integration.ConcurrentClaim")
            .ToListAsync();

        var row = Assert.Single(rows);
        Assert.Equal(OutboxMessageStatus.Processing, row.Status);
        Assert.Equal(claimed[0].LockId, row.LockId);

        await verification.Database.ExecuteSqlRawAsync(
            "DELETE FROM OutboxMessages WHERE EventType = 'Integration.ConcurrentClaim';");
    }

    private static async Task<IReadOnlyList<OutboxMessage>> ClaimAfterStartAsync(
        OutboxRepository repository,
        string lockId,
        DateTime now,
        DateTime lockUntil,
        Task start)
    {
        await start;
        return await repository.ClaimBatchAsync(1, lockId, now, lockUntil);
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
