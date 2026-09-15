namespace XFramework.Application.Events;

public sealed record EventEnvelope
{
    public Guid EventId { get; init; }

    public string EventType { get; init; } = null!;

    public int EventVersion { get; init; }

    public string Payload { get; init; } = null!;

    public DateTime OccurredOnUtc { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public int RetryCount { get; init; }

    public string? LastError { get; init; }

    public DateTime? LastAttemptOnUtc { get; init; }
}