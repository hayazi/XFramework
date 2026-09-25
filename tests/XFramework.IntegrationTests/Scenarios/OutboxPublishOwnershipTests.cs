using Microsoft.Extensions.Options;
using Xunit;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Application.Outbox;
using XFramework.EntityFrameworkCore.Outbox;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class OutboxPublishOwnershipTests
{
    [Fact]
    public async Task Lost_ownership_after_publish_should_allow_the_next_worker_to_publish_the_same_event_id()
    {
        var message = CreateMessage();
        var repository = new ReclaimingOutboxRepository(message);
        var eventBus = new RecordingEventBus();
        var processor = CreateProcessor(repository, eventBus);

        await processor.ProcessBatchAsync(1);
        await processor.ProcessBatchAsync(1);

        Assert.Equal(2, eventBus.Published.Count);
        Assert.Equal(message.EventId, eventBus.Published[0].EventId);
        Assert.Equal(message.EventId, eventBus.Published[1].EventId);
        Assert.NotEqual(eventBus.Published[0].EventId, Guid.Empty);

        Assert.Equal(2, repository.ClaimCount);
        Assert.Equal(2, repository.RenewLeaseCount);
        Assert.Equal(2, repository.CompletionAttempts);
        Assert.Equal(1, repository.SuccessfulCompletions);
    }

    [Fact]
    public async Task Lost_ownership_after_publish_must_not_trigger_a_local_republish()
    {
        var message = CreateMessage();
        var repository = new ReclaimingOutboxRepository(message);
        var eventBus = new RecordingEventBus();
        var processor = CreateProcessor(repository, eventBus);

        await processor.ProcessBatchAsync(1);

        Assert.Single(eventBus.Published);
        Assert.Equal(message.EventId, eventBus.Published[0].EventId);
        Assert.Equal(1, repository.CompletionAttempts);
        Assert.Equal(0, repository.SuccessfulCompletions);
        Assert.Equal(0, repository.RetryCount);
        Assert.Equal(0, repository.FailedCount);
    }

    private static OutboxProcessor CreateProcessor(
        ReclaimingOutboxRepository repository,
        RecordingEventBus eventBus)
        => new(
            repository,
            eventBus,
            new NeverRetryPolicy(),
            Options.Create(new OutboxOptions
            {
                LeaseMinutes = 2
            }));

    private static OutboxMessage CreateMessage() => new()
    {
        Id = Guid.NewGuid(),
        EventId = Guid.NewGuid(),
        EventType = "Integration.OutboxPublishOwnership",
        EventVersion = 1,
        Payload = "{}",
        Status = OutboxMessageStatus.Pending,
        RetryCount = 0,
        CreatedOnUtc = DateTime.UtcNow,
        OccurredOnUtc = DateTime.UtcNow
    };

    private sealed class RecordingEventBus : IEventBus
    {
        public List<EventEnvelope> Published { get; } = [];

        public Task PublishAsync(
            EventEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            Published.Add(envelope);
            return Task.CompletedTask;
        }
    }

    private sealed class NeverRetryPolicy : IOutboxRetryPolicy
    {
        public bool ShouldRetry(int retryCount, Exception exception) => false;

        public TimeSpan GetDelay(int retryCount) => TimeSpan.Zero;
    }

    private sealed class ReclaimingOutboxRepository : IOutboxRepository
    {
        private readonly OutboxMessage _message;
        private int _claimCount;

        public ReclaimingOutboxRepository(OutboxMessage message)
        {
            _message = message;
        }

        public int ClaimCount => _claimCount;
        public int RenewLeaseCount { get; private set; }
        public int CompletionAttempts { get; private set; }
        public int SuccessfulCompletions { get; private set; }
        public int RetryCount { get; private set; }
        public int FailedCount { get; private set; }

        public Task<int> ReleaseExpiredLeasesAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
            int batchSize,
            string lockId,
            DateTime nowUtc,
            DateTime lockedUntilUtc,
            CancellationToken cancellationToken = default)
        {
            _claimCount++;
            _message.Status = OutboxMessageStatus.Processing;
            _message.LockId = lockId;
            _message.LockedUntilUtc = lockedUntilUtc;
            return Task.FromResult<IReadOnlyList<OutboxMessage>>([_message]);
        }

        public Task<bool> RenewLeaseAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            DateTime lockedUntilUtc,
            CancellationToken cancellationToken = default)
        {
            RenewLeaseCount++;
            _message.LockedUntilUtc = lockedUntilUtc;
            return Task.FromResult(true);
        }

        public Task<bool> MarkCompletedAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            DateTime completedOnUtc,
            CancellationToken cancellationToken = default)
        {
            CompletionAttempts++;

            // Worker A publishes successfully but loses ownership before completion.
            // Worker B later claims the same EventId and is allowed to complete it.
            if (CompletionAttempts == 1)
                return Task.FromResult(false);

            SuccessfulCompletions++;
            _message.Status = OutboxMessageStatus.Completed;
            _message.LockId = null;
            _message.LockedUntilUtc = null;
            _message.ProcessedOnUtc = completedOnUtc;
            return Task.FromResult(true);
        }

        public Task<bool> MarkRetryAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            DateTime nextAttemptOnUtc,
            string error,
            CancellationToken cancellationToken = default)
        {
            RetryCount++;
            return Task.FromResult(true);
        }

        public Task<bool> MarkFailedAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            string error,
            CancellationToken cancellationToken = default)
        {
            FailedCount++;
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            if (_message.Status == OutboxMessageStatus.Pending)
                return Task.FromResult<IReadOnlyList<OutboxMessage>>([_message]);
            return Task.FromResult<IReadOnlyList<OutboxMessage>>([]);
        }

        public Task<IReadOnlyList<OutboxMessage>> GetFailedAsync(
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            if (_message.Status == OutboxMessageStatus.Failed)
                return Task.FromResult<IReadOnlyList<OutboxMessage>>([_message]);
            return Task.FromResult<IReadOnlyList<OutboxMessage>>([]);
        }

        public Task<IReadOnlyList<OutboxMessage>> GetProcessingAsync(
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            if (_message.Status == OutboxMessageStatus.Processing)
                return Task.FromResult<IReadOnlyList<OutboxMessage>>([_message]);
            return Task.FromResult<IReadOnlyList<OutboxMessage>>([]);
        }

        public Task<OutboxMessage?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (_message.Id == id)
                return Task.FromResult<OutboxMessage?>(_message);
            return Task.FromResult<OutboxMessage?>(null);
        }

        public Task<int> GetCountByStatusAsync(
            OutboxMessageStatus status,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_message.Status == status ? 1 : 0);
        }
    }
}
