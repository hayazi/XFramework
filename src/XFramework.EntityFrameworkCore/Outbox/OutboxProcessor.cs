using Microsoft.Extensions.Logging;
using XFramework.Application.Events;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxRepository _repository;
    private readonly IEventBus _eventBus;

    public OutboxProcessor(
        IOutboxRepository repository,
        IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task ProcessBatchAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        await _repository.ReleaseExpiredLeasesAsync(
            now,
            cancellationToken);

        var lockId = Guid.NewGuid().ToString("N");

        var messages =
            await _repository.ClaimBatchAsync(
                batchSize,
                lockId,
                now,
                now.AddMinutes(2),
                cancellationToken);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(
                message,
                lockId,
                cancellationToken);
        }
    }
    private async Task ProcessMessageAsync(
        OutboxMessage message,
        string lockId,
        CancellationToken cancellationToken)
    {
        try
        {
            var envelope = BuildEnvelope(message);

            await _eventBus.PublishAsync(
                envelope,
                cancellationToken);

            await _repository.MarkCompletedAsync(
                message.Id,
                lockId,
                DateTime.UtcNow,
                cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleFailureAsync(
                message,
                lockId,
                ex,
                cancellationToken);
        }
    }
}