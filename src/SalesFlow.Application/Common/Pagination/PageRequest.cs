using SalesFlow.Application.Common.Errors;

namespace SalesFlow.Application.Common.Pagination;
public sealed record PageRequest
{
    public int Page { get; }
    public int PageSize { get; }
    public PageRequest(int page = 1, int pageSize = 20)
    {
        if (page < 1) throw new ValidationException("Page must be at least 1.");
        if (pageSize < 1 || pageSize > 100) throw new ValidationException("PageSize must be between 1 and 100.");
        Page = page;
        PageSize = pageSize;
    }
}
