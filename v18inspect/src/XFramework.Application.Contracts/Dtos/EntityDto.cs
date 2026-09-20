namespace XFramework.Application.Contracts.Dtos;

public abstract class EntityDto<TKey>
{
    public TKey Id { get; set; } = default!;
}