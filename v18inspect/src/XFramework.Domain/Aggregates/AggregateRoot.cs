using XFramework.Domain.Abstractions;
using XFramework.Domain.Entities;

namespace XFramework.Domain.Aggregates;

public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
{
}
