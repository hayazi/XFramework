namespace XFramework.EntityFrameworkCore.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime OccurredOnUtc { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public int RetryCount { get; set; }

    public string? LastError { get; set; }

    public DateTime? NextAttemptOnUtc { get; set; }

    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;

    public string? CorrelationId { get; set; }

    public string? CausationId { get; set; }

    public string? AggregateType { get; set; }

    public string? AggregateId { get; set; }
}