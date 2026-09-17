namespace XFramework.Application.Contracts.Dtos;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public bool HasItems => Items.Count > 0;

    public PagedResult(IReadOnlyList<T> items, int totalCount)
    { Items=items; TotalCount=totalCount; }

    public static PagedResult<T> Empty() => new(Array.Empty<T>(), 0);
}
