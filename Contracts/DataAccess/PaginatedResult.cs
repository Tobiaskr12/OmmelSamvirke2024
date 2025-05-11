namespace Contracts.DataAccess;

public class PaginatedResult<T>
{
    public required List<T> Items { get; set; } = [];
    public required int ItemsCount { get; set; }
    public required int PageCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
