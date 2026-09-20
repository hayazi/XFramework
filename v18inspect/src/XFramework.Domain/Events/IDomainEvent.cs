namespace XFramework.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
    Guid? CorrelationId { get; }
    Guid? CausationId { get; }
}
