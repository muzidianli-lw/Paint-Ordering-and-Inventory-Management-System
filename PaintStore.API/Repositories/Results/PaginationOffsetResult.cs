using System;

namespace PaintStore.API.Repositories.Results;

public class PaginationOffsetResult<T>
{
    public List<T> Items { get; set; } = null!;
    public int TotalCount { get; set; }
}
