using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XFramework.Application.Contracts.Outbox;

public sealed record OutboxMessageSummary(
    Guid Id,
    Guid EventId,
    string EventType,
    int EventVersion,
    OutboxMessageStatus Status,
    int RetryCount,
    DateTime? NextAttemptOnUtc,
    DateTime CreatedOnUtc,
    DateTime OccurredOnUtc,
    string? AggregateType,
    string? AggregateId,
    string? ModuleName,
    string? CorrelationId,
    string? CausationId);

public sealed record OutboxMessageDetail(
    Guid Id,
    Guid EventId,
    string EventType,
    int EventVersion,
    OutboxMessageStatus Status,
    int RetryCount,
    DateTime? NextAttemptOnUtc,
    string? LockId,
    DateTime? LockedUntilUtc,
    string? LastError,
    DateTime CreatedOnUtc,
    DateTime OccurredOnUtc,
    DateTime? ProcessedOnUtc,
    DateTime? FailedOnUtc,
    string? AggregateType,
    string? AggregateId,
    string? ModuleName,
    string? CorrelationId,
    string? CausationId,
    string Payload,
    string? Headers);

public sealed record OutboxStats(
    int PendingCount,
    int ProcessingCount,
    int CompletedCount,
    int FailedCount);

public interface IOutboxAdminService
{
    Task<IReadOnlyList<OutboxMessageSummary>> GetPendingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessageSummary>> GetFailedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessageSummary>> GetProcessingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<OutboxMessageDetail?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> RetryAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ForceCompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<OutboxStats> GetStatsAsync(
        CancellationToken cancellationToken = default);
}