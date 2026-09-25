using Xunit;
using Microsoft.Extensions.Options;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Application.Outbox;
using XFramework.EntityFrameworkCore.Outbox;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class OutboxProcessorReliabilityTests
{
    [Fact]
    public async Task Successful_publish_should_mark_message_completed()
    {
        var repository = new FakeOutboxRepository(CreateMessage());
        var eventBus = new FakeEventBus();
        var retryPolicy = new FakeRetryPolicy(shouldRetry: true);
        var processor = CreateProcessor(repository, eventBus, retryPolicy);

        await processor.ProcessBatchAsync(10);

        Assert.Single(eventBus.Published);
        Assert.Single(repository.Completed);
        Assert.Empty(repository.Retried);
        Assert.Empty(repository.Failed);
    }

    [Fact]
    public async Task Retryable_publish_failure_should_mark_message_for_retry()
    {
        var repository = new FakeOutboxRepository(CreateMessage());
        var eventBus = new FakeEventBus(new TimeoutException("RabbitMQ timeout."));
        var retryPolicy = new FakeRetryPolicy(shouldRetry: true, TimeSpan.FromSeconds(5));
        var processor = CreateProcessor(repository, eventBus, retryPolicy);

        await processor.ProcessBatchAsync(10);

        Assert.Empty(repository.Completed);
        var retry = Assert.Single(repository.Retried);
        Assert.Equal(repository.Message.Id, retry.MessageId);
        Assert.Contains("RabbitMQ timeout.", retry.ErrorMessage);
        Assert.InRange(retry.NextAttemptOnUtc, DateTime.UtcNow.AddSeconds(3), DateTime.UtcNow.AddSeconds(8));
        Assert.Empty(repository.Failed);
    }

    [Fact]
    public async Task Non_retryable_publish_failure_should_mark_message_failed()
    {
        var repository = new FakeOutboxRepository(CreateMessage());
        var eventBus = new FakeEventBus(new InvalidOperationException("Invalid event contract."));
        var retryPolicy = new FakeRetryPolicy(shouldRetry: false);
        var processor = CreateProcessor(repository, eventBus, retryPolicy);

        await processor.ProcessBatchAsync(10);

        Assert.Empty(repository.Completed);
        Assert.Empty(repository.Retried);
        var failed = Assert.Single(repository.Failed);
        Assert.Equal(repository.Message.Id, failed.MessageId);
        Assert.Contains("Invalid event contract.", failed.ErrorMessage);
    }

    [Fact]
    public async Task Processor_should_renew_the_lease_before_publishing()
    {
        var message = CreateMessage();
        var repository = new FakeOutboxRepository(message);
        var eventBus = new FakeEventBus();
        var processor = CreateProcessor(repository, eventBus);

        await processor.ProcessBatchAsync(10);

        Assert.Equal(1, repository.RenewLeaseCallCount);
        Assert.Single(eventBus.Published);
        Assert.NotNull(repository.LastRenewedUntilUtc);
        Assert.True(repository.LastRenewedUntilUtc > DateTime.UtcNow.AddSeconds(30));
    }

    [Fact]
    public async Task Processor_should_not_publish_when_lease_was_lost()
    {
        var message = CreateMessage();
        var repository = new FakeOutboxRepository(message)
        {
            RenewLeaseSucceeds = false
        };
        var eventBus = new FakeEventBus();
        var processor = CreateProcessor(repository, eventBus);

        await processor.ProcessBatchAsync(10);

        Assert.Equal(1, repository.RenewLeaseCallCount);
        Assert.Empty(eventBus.Published);
        Assert.Empty(repository.Completed);
        Assert.Empty(repository.Retried);
        Assert.Empty(repository.Failed);
    }

    [Fact]
    public async Task Lost_ownership_after_publish_should_not_retry_or_fail_the_already_published_event()
    {
        var repository = new FakeOutboxRepository(CreateMessage())
        {
            CompleteSucceeds = false
        };
        var eventBus = new FakeEventBus();
        var retryPolicy = new FakeRetryPolicy(shouldRetry: true);
        var processor = CreateProcessor(repository, eventBus, retryPolicy);

        await processor.ProcessBatchAsync(10);

        Assert.Single(eventBus.Published);
        Assert.Empty(repository.Completed);
        Assert.Empty(repository.Retried);
        Assert.Empty(repository.Failed);
    }


    [Fact]
    public async Task Lost_retry_ownership_should_not_be_reported_as_a_successful_retry()
    {
        var repository = new FakeOutboxRepository(CreateMessage())
        {
            RetrySucceeds = false
        };
        var eventBus = new FakeEventBus(new TimeoutException("RabbitMQ timeout."));
        var retryPolicy = new FakeRetryPolicy(shouldRetry: true);
        var processor = CreateProcessor(repository, eventBus, retryPolicy);

        await processor.ProcessBatchAsync(10);

        Assert.Empty(repository.Retried);
        Assert.Empty(repository.Failed);
        Assert.Equal(1, repository.RetryAttempts);
    }

    [Fact]
    public async Task Lost_failure_ownership_should_not_be_reported_as_a_successful_failure()
    {
        var repository = new FakeOutboxRepository(CreateMessage())
        {
            FailSucceeds = false
        };
        var eventBus = new FakeEventBus(new InvalidOperationException("Invalid event contract."));
        var retryPolicy = new FakeRetryPolicy(shouldRetry: false);
        var processor = CreateProcessor(repository, eventBus, retryPolicy);

        await processor.ProcessBatchAsync(10);

        Assert.Empty(repository.Failed);
        Assert.Empty(repository.Retried);
        Assert.Equal(1, repository.FailureAttempts);
    }

    [Fact]
    public async Task Cancellation_during_publish_should_propagate_without_marking_retry_or_failure()
    {
        var repository = new FakeOutboxRepository(CreateMessage());
        using var cancellationSource = new CancellationTokenSource();
        var eventBus = new FakeEventBus(cancellationSource);
        var retryPolicy = new FakeRetryPolicy(shouldRetry: true);
        var processor = CreateProcessor(repository, eventBus, retryPolicy);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => processor.ProcessBatchAsync(10, cancellationSource.Token));

        Assert.Empty(repository.Completed);
        Assert.Empty(repository.Retried);
        Assert.Empty(repository.Failed);
    }

    private static OutboxProcessor CreateProcessor(
        FakeOutboxRepository repository,
        FakeEventBus eventBus,
        FakeRetryPolicy? retryPolicy = null)
    {
        return new OutboxProcessor(
            repository,
            eventBus,
            retryPolicy ?? new FakeRetryPolicy(shouldRetry: false),
            Options.Create(new OutboxOptions
            {
                LeaseMinutes = 2
            }));
    }

    private static OutboxMessage CreateMessage() => new()
    {
        Id = Guid.NewGuid(),
        EventId = Guid.NewGuid(),
        EventType = "Integration.OutboxProcessorTest",
        EventVersion = 1,
        Payload = "{}",
        Status = OutboxMessageStatus.Pending,
        RetryCount = 0,
        CreatedOnUtc = DateTime.UtcNow,
        OccurredOnUtc = DateTime.UtcNow
    };

    private sealed class FakeEventBus : IEventBus
    {
        private readonly Exception? _exception;
        private readonly CancellationTokenSource? _cancellationSource;

        public FakeEventBus(Exception? exception = null)
        {
            _exception = exception;
        }

        public FakeEventBus(CancellationTokenSource cancellationSource)
        {
            _cancellationSource = cancellationSource;
        }

        public List<EventEnvelope> Published { get; } = [];

        public async Task PublishAsync(
            EventEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            if (_cancellationSource is not null)
            {
                _cancellationSource.Cancel();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return;
            }

            if (_exception is not null)
                throw _exception;

            Published.Add(envelope);
            await Task.CompletedTask;
        }
    }

    private sealed class FakeRetryPolicy : IOutboxRetryPolicy
    {
        private readonly bool _shouldRetry;
        private readonly TimeSpan _delay;

        public FakeRetryPolicy(bool shouldRetry, TimeSpan? delay = null)
        {
            _shouldRetry = shouldRetry;
            _delay = delay ?? TimeSpan.FromSeconds(5);
        }

        public bool ShouldRetry(int retryCount, Exception exception) => _shouldRetry;

        public TimeSpan GetDelay(int retryCount) => _delay;
    }

    private sealed class FakeOutboxRepository : IOutboxRepository
    {
        public FakeOutboxRepository(OutboxMessage message)
        {
            Message = message;
        }

        public OutboxMessage Message { get; }
        public List<Guid> Completed { get; } = [];
        public List<RetryRecord> Retried { get; } = [];
        public List<FailedRecord> Failed { get; } = [];
        public bool RenewLeaseSucceeds { get; set; } = true;
        public bool CompleteSucceeds { get; set; } = true;
        public bool RetrySucceeds { get; set; } = true;
        public bool FailSucceeds { get; set; } = true;
        public int RetryAttempts { get; private set; }
        public int FailureAttempts { get; private set; }
        public int RenewLeaseCallCount { get; private set; }
        public DateTime? LastRenewedUntilUtc { get; private set; }

        public Task ReleaseExpiredLeasesAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
            int batchSize,
            string lockId,
            DateTime nowUtc,
            DateTime lockedUntilUtc,
            CancellationToken cancellationToken = default)
        {
            Message.Status = OutboxMessageStatus.Processing;
            Message.LockId = lockId;
            Message.LockedUntilUtc = lockedUntilUtc;
            return Task.FromResult<IReadOnlyList<OutboxMessage>>([Message]);
        }

        public Task<bool> RenewLeaseAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            DateTime lockedUntilUtc,
            CancellationToken cancellationToken = default)
        {
            RenewLeaseCallCount++;
            LastRenewedUntilUtc = lockedUntilUtc;
            Message.LockedUntilUtc = lockedUntilUtc;
            return Task.FromResult(RenewLeaseSucceeds);
        }

        public Task<bool> MarkCompletedAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            DateTime completedOnUtc,
            CancellationToken cancellationToken = default)
        {
            if (!CompleteSucceeds)
                return Task.FromResult(false);

            Completed.Add(messageId);
            Message.Status = OutboxMessageStatus.Completed;
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
            RetryAttempts++;
            if (!RetrySucceeds)
                return Task.FromResult(false);

            Retried.Add(new RetryRecord(messageId, nextAttemptOnUtc, error));
            Message.Status = OutboxMessageStatus.Pending;
            Message.RetryCount++;
            Message.NextAttemptOnUtc = nextAttemptOnUtc;
            Message.LastError = error;
            return Task.FromResult(true);
        }

        public Task<bool> MarkFailedAsync(
            Guid messageId,
            string lockId,
            DateTime nowUtc,
            string error,
            CancellationToken cancellationToken = default)
        {
            FailureAttempts++;
            if (!FailSucceeds)
                return Task.FromResult(false);

            Failed.Add(new FailedRecord(messageId, error));
            Message.Status = OutboxMessageStatus.Failed;
            Message.RetryCount++;
            Message.LastError = error;
            return Task.FromResult(true);
        }

        public sealed record RetryRecord(
            Guid MessageId,
            DateTime NextAttemptOnUtc,
            string ErrorMessage);

        public sealed record FailedRecord(
            Guid MessageId,
            string ErrorMessage);
    }
}
