namespace XFramework.Core.Abstractions;

public interface IEntity<TKey>
{
    TKey Id { get; }
}