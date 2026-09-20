using XFramework.Application.Abstractions;
using XFramework.Application.Events;

namespace XFramework.IntegrationTests.Infrastructure;

public sealed class TestIdempotencyService : IIdempotencyService
{
    private readonly object _sync = new();
    private readonly HashSet<string> _processed = new(StringComparer.Ordinal);

    public Task<bool> TryBeginProcessingAsync(
        Guid eventId,
        string handlerName,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var key = $"{eventId:N}|{handlerName}";

        lock (_sync)
        {
            return Task.FromResult(_processed.Add(key));
        }
    }
}

public sealed class TestUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }
    public int CommitCallCount { get; private set; }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(0);
    }

    public Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IUnitOfWorkTransaction>(
            new TestUnitOfWorkTransaction(this));
    }

    private sealed class TestUnitOfWorkTransaction(TestUnitOfWork owner)
        : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            owner.CommitCallCount++;
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

public sealed class TestEventRetryPublisher : IEventRetryPublisher
{
    public int CallCount { get; private set; }

    public Task PublishRetryAsync(
        XFramework.Application.Contracts.Events.EventEnvelope envelope,
        string routingKey,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.CompletedTask;
    }
}

public sealed class TestEventDeadLetterPublisher : IEventDeadLetterPublisher
{
    public int CallCount { get; private set; }
    public Exception? LastException { get; private set; }
    public string? LastExceptionText { get; private set; }

    public Task PublishAsync(
        XFramework.Application.Contracts.Events.EventEnvelope envelope,
        string routingKey,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastException = exception;
        LastExceptionText = exception.ToString();
        return Task.CompletedTask;
    }

    public Task PublishRawAsync(
        byte[] body,
        string? routingKey,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastException = exception;
        LastExceptionText = exception.ToString();
        return Task.CompletedTask;
    }
}
