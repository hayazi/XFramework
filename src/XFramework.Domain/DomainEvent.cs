namespace XFramework.Core.Domain;

public abstract record DomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}