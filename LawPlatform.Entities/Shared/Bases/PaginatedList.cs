using Microsoft.EntityFrameworkCore;

namespace LawPlatform.Entities.Shared;


public class PaginatedList<T>(List<T> items, int pageNumber, int pageSize, int count)
{
    public List<T> Items { get; private set; } = items;
    public int PageNumber { get; private set; } = pageNumber;
    public int PageSize { get; set; }
    public int TotalCount { get; set; } = items.Count;
    public int TotalPages { get; private set; } = (int)Math.Ceiling(count / (double)pageSize);

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, pageNumber, pageSize, count);
    }

}
