using XFramework.Core.Abstractions;

namespace XFramework.Core.Domain;

public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
{
}