namespace XFramework.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
}
