namespace XFramework.Application.Contracts.Dtos;

public class PagedAndSortedRequestDto
{
    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; } = 50;

    public string? Sorting { get; set; }
}