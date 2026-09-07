namespace XFramework.Application.Contracts.Dtos;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }

    public int TotalCount { get; }

    public bool HasItems => Items.Count > 0;

    public PagedResult(
        int totalCount,
        IReadOnlyList<T> items)
    {
        TotalCount = totalCount;
        Items = items;
    }

    public static PagedResult<T> Empty()
    {
        return new PagedResult<T>(
            0,
            Array.Empty<T>());
    }
}