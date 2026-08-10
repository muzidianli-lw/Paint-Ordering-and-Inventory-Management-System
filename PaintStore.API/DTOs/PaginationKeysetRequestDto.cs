using System;
using System.ComponentModel.DataAnnotations;

namespace PaintStore.API.DTOs;

public sealed class PaginationKeysetRequestDto
{
    [Range(1, int.MaxValue)]
    public int? LastPageEndId { get; set; }

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
