
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Shared;
public class RequestFilters<TSortColumn>
    where TSortColumn : struct, Enum
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchValue { get; init; }
    public string? CreatedAt { get; set; }
    public TSortColumn? SortColumn { get; set; }
    public SortDirection? SortDirection { get; init; } = Utilities.Enums.SortDirection.DESC;
}

