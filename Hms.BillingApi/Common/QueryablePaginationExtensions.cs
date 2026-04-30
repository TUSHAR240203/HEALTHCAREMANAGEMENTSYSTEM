using Microsoft.EntityFrameworkCore;

namespace Hms.BillingApi.Common;

public static class QueryablePaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, pagination.PageNumber);
        var pageSize = Math.Max(1, pagination.PageSize);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
