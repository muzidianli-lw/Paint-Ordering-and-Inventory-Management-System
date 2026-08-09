using System;

namespace PaintStore.API.DTOs;

public sealed class PaginationResponseDto<T>
{
    public List<T> Items { get; set; } = null!;
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
