namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; } = null!;
    public int EventVersion { get; set; }
    public string? AggregateType { get; set; }
    public string? AggregateId { get; set; }
    public string? ModuleName { get; set; }
    public string Payload { get; set; } = null!;
    public string? Headers { get; set; }
    public OutboxMessageStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextAttemptOnUtc { get; set; }
    public string? LockId { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public DateTime? FailedOnUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
}
