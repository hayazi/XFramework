namespace XFramework.Domain.Abstractions;

public interface IEntity<TKey>
{
    TKey Id { get; }
}
