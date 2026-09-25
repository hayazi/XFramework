using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XFramework.Application.Contracts.Outbox;
using XFramework.EntityFrameworkCore.Outbox;
using XFramework.Application.Abstractions;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxAdminService : IOutboxAdminService
{
    private readonly IOutboxRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OutboxAdminService> _logger;
    private const string AdminLockId = "admin-override";

    public OutboxAdminService(
        IOutboxRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<OutboxAdminService>? logger = null)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<OutboxAdminService>.Instance;
    }

    public async Task<IReadOnlyList<OutboxMessageSummary>> GetPendingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var messages = await _repository.GetPendingAsync(page * pageSize, pageSize, cancellationToken);
        return messages.Select(MapToSummary).ToList();
    }

    public async Task<IReadOnlyList<OutboxMessageSummary>> GetFailedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var messages = await _repository.GetFailedAsync(page * pageSize, pageSize, cancellationToken);
        return messages.Select(MapToSummary).ToList();
    }

    public async Task<IReadOnlyList<OutboxMessageSummary>> GetProcessingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var messages = await _repository.GetProcessingAsync(page * pageSize, pageSize, cancellationToken);
        return messages.Select(MapToSummary).ToList();
    }

    public async Task<OutboxMessageDetail?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetByIdAsync(id, cancellationToken);
        return message is null ? null : MapToDetail(message);
    }

    public async Task<bool> RetryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetByIdAsync(id, cancellationToken);
        if (message is null)
        {
            _logger.LogWarning("Outbox message {MessageId} not found for retry", id);
            return false;
        }

        if (message.Status != OutboxMessageStatus.Failed)
        {
            _logger.LogWarning("Outbox message {MessageId} is not in Failed status (current: {Status})", id, message.Status);
            return false;
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var now = DateTime.UtcNow;
            var retried = await _repository.MarkRetryAsync(
                message.Id,
                message.LockId ?? AdminLockId,
                now,
                now,
                "Manual retry triggered by admin",
                cancellationToken);

            if (retried)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Outbox message {MessageId} manually retried by admin", id);
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Outbox message {MessageId} retry failed - ownership lost", id);
            }

            return retried;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> ForceCompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetByIdAsync(id, cancellationToken);
        if (message is null)
        {
            _logger.LogWarning("Outbox message {MessageId} not found for force complete", id);
            return false;
        }

        if (message.Status == OutboxMessageStatus.Completed)
        {
            _logger.LogInformation("Outbox message {MessageId} already completed", id);
            return true;
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var now = DateTime.UtcNow;
            var completed = await _repository.MarkCompletedAsync(
                message.Id,
                message.LockId ?? AdminLockId,
                now,
                now,
                cancellationToken);

            if (completed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogWarning("Outbox message {MessageId} force-completed by admin (previous status: {Status})", id, message.Status);
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Outbox message {MessageId} force complete failed - ownership lost", id);
            }

            return completed;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OutboxStats> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var pending = await _repository.GetCountByStatusAsync(OutboxMessageStatus.Pending, cancellationToken);
        var processing = await _repository.GetCountByStatusAsync(OutboxMessageStatus.Processing, cancellationToken);
        var completed = await _repository.GetCountByStatusAsync(OutboxMessageStatus.Completed, cancellationToken);
        var failed = await _repository.GetCountByStatusAsync(OutboxMessageStatus.Failed, cancellationToken);

        return new OutboxStats(pending, processing, completed, failed);
    }

    private static OutboxMessageSummary MapToSummary(OutboxMessage m) => new(
        m.Id,
        m.EventId,
        m.EventType,
        m.EventVersion,
        ToContractStatus(m.Status),
        m.RetryCount,
        m.NextAttemptOnUtc,
        m.CreatedOnUtc,
        m.OccurredOnUtc,
        m.AggregateType,
        m.AggregateId,
        m.ModuleName,
        m.CorrelationId,
        m.CausationId);

    private static OutboxMessageDetail MapToDetail(OutboxMessage m) => new(
        m.Id,
        m.EventId,
        m.EventType,
        m.EventVersion,
        ToContractStatus(m.Status),
        m.RetryCount,
        m.NextAttemptOnUtc,
        m.LockId,
        m.LockedUntilUtc,
        m.LastError,
        m.CreatedOnUtc,
        m.OccurredOnUtc,
        m.ProcessedOnUtc,
        m.FailedOnUtc,
        m.AggregateType,
        m.AggregateId,
        m.ModuleName,
        m.CorrelationId,
        m.CausationId,
        m.Payload,
        m.Headers);

    private static XFramework.Application.Contracts.Outbox.OutboxMessageStatus ToContractStatus(OutboxMessageStatus status) =>
        status switch
        {
            OutboxMessageStatus.Pending => XFramework.Application.Contracts.Outbox.OutboxMessageStatus.Pending,
            OutboxMessageStatus.Processing => XFramework.Application.Contracts.Outbox.OutboxMessageStatus.Processing,
            OutboxMessageStatus.Completed => XFramework.Application.Contracts.Outbox.OutboxMessageStatus.Completed,
            OutboxMessageStatus.Failed => XFramework.Application.Contracts.Outbox.OutboxMessageStatus.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown outbox message status")
        };
}