using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Application.Outbox;
using XFramework.EntityFrameworkCore.Outbox;
using Microsoft.Extensions.Options;
using Xunit;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class OutboxCrashRecoveryTests
{
    [Fact]
    public async Task Expired_processing_lease_should_be_recovered_and_published()
    {
        var eventId = Guid.NewGuid();
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            EventType = "Integration.CrashRecovery",
            EventVersion = 1,
            Payload = "{}",
            Status = OutboxMessageStatus.Processing,
            RetryCount = 0,
            CreatedOnUtc = DateTime.UtcNow.AddMinutes(-2),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-2),
            LockId = "dead-worker-lock",
            LockedUntilUtc = DateTime.UtcNow.AddMinutes(-1)
        };

        var repository = new CrashRecoveryOutboxRepository(message);
        var eventBus = new RecordingEventBus();
        var options = Options.Create(new OutboxOptions { LeaseMinutes = 1 });
        var processor = new OutboxProcessor(
            repository,
            eventBus,
            new TestOutboxRetryPolicy(),
            options);

        await processor.ProcessBatchAsync(10);

        Assert.Equal(OutboxMessageStatus.Completed, message.Status);
        Assert.Equal(1, repository.ReleaseExpiredLeaseCallCount);
        Assert.Equal(1, repository.ClaimCallCount);
        Assert.Equal(1, repository.MarkCompletedCallCount);
        Assert.Single(eventBus.Published);
        Assert.Equal(eventId, eventBus.Published[0].EventId);
    }

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

    private sealed class TestOutboxRetryPolicy : IOutboxRetryPolicy
    {
        public bool ShouldRetry(int retryCount, Exception exception) => false;

        public TimeSpan GetDelay(int retryCount) => TimeSpan.Zero;
    }

    private sealed class CrashRecoveryOutboxRepository(OutboxMessage message)
        : IOutboxRepository
    {
        private readonly OutboxMessage _message = message;

        public int ReleaseExpiredLeaseCallCount { get; private set; }
        public int ClaimCallCount { get; private set; }
        public int MarkCompletedCallCount { get; private set; }

        public Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
            int batchSize,
            string lockId,
            DateTime nowUtc,
            DateTime lockedUntilUtc,
            CancellationToken cancellationToken = default)
        {
            ClaimCallCount++;

            if (_message.Status != OutboxMessageStatus.Pending)
                return Task.FromResult<IReadOnlyList<OutboxMessage>>([]);

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
            Assert.Equal(_message.Id, messageId);
            Assert.Equal(lockId, _message.LockId);
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
            MarkCompletedCallCount++;
            Assert.Equal(_message.Id, messageId);
            Assert.Equal(lockId, _message.LockId);

            _message.Status = OutboxMessageStatus.Completed;
            _message.ProcessedOnUtc = completedOnUtc;
            _message.LockId = null;
            _message.LockedUntilUtc = null;
            return Task.FromResult(true);
        }

        public Task<bool> MarkRetryAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            DateTime nextAttemptOnUtc,
            string error,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> MarkFailedAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            string error,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<int> ReleaseExpiredLeasesAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            ReleaseExpiredLeaseCallCount++;

            int released = 0;
            if (_message.Status == OutboxMessageStatus.Processing &&
                _message.LockedUntilUtc <= nowUtc)
            {
                _message.Status = OutboxMessageStatus.Pending;
                _message.LockId = null;
                _message.LockedUntilUtc = null;
                released = 1;
            }

            return Task.FromResult(released);
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
