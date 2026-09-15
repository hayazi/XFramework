using XFramework.Core.Abstractions;

namespace XFramework.Core.Domain;

public abstract class Entity<TKey> : IEntity<TKey>
{
    public TKey Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other)
            return false;

        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TKey>.Default.GetHashCode(Id);
    }
}