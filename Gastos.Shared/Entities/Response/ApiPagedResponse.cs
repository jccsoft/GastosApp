using Microsoft.EntityFrameworkCore;

namespace Gastos.Shared.Entities.Response;

public sealed record ApiPagedResponse<T>
{
    public string UserId { get; set; } = string.Empty;
    public required List<T> Items { get; init; }
    public int TotalItems { get; init; }

    public int Page { get; init; }
    public int PageSize { get; init; }


    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;


    public static async Task<ApiPagedResponse<T>> CreateAsync(
        string userId,
        IQueryable<T> query,
        int page,
        int pageSize)
    {
        int totalCount = await query.CountAsync();

        List<T> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ApiPagedResponse<T>
        {
            UserId = userId,
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount
        };
    }
}
