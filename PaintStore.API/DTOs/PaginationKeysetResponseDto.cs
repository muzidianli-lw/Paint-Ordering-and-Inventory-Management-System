using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PaintStore.API.DTOs;

public sealed class PaginationKeysetResponseDto<T>
{
    public List<T> Items { get; set; } = null!;
    public bool HasNextPage { get; set; }
    public int? ThisPageEndId { get; set; }
}
