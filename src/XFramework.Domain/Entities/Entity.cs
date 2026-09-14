using XFramework.Domain.Events;

namespace XFramework.Domain.Entities;

public abstract class Entity<TKey> : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public TKey Id { get; protected set; } = default!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}