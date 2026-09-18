namespace MovieCatalog.Application.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public PagedResult(IEnumerable<T> items, long totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
