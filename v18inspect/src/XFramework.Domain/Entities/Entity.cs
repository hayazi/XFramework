using XFramework.Domain.Abstractions;
using XFramework.Domain.Events;

namespace XFramework.Domain.Entities;

public abstract class Entity<TKey> : IEntity<TKey>, IHasDomainEvents
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

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other) return false;
        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() =>
        EqualityComparer<TKey>.Default.GetHashCode(Id!);
}
