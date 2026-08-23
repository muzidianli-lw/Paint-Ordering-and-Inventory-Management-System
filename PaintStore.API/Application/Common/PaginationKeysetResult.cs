using System;

namespace PaintStore.API.Application.Common;

public class PaginationKeysetResult<T>
{
    public List<T> Items { get; set; } = null!;
    public bool HasNextPage { get; set; }
    public int? ThisPageEndId { get; set; }
}
