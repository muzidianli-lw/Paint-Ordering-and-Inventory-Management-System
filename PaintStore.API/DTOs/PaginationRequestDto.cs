using System;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace PaintStore.API.DTOs;

public class PaginationRequestDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
