using System;

namespace PaintStore.API.Application.Common;

public class PaginationOffsetQueryResult<T>
{
    public List<T> Items { get; set; } = null!;
    public int TotalCount { get; set; }
}
