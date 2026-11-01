namespace SalesFlow.Application.Common.Pagination;
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public PagedResult<TTarget> Map<TTarget>(Func<T, TTarget> mapper) => new(Items.Select(mapper).ToArray(), Page, PageSize, TotalCount);
}
